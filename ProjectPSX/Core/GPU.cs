using System;
using System.Runtime.CompilerServices;
using ProjectPSX.Core.Graphics;
using ProjectPSX.Graphics;

namespace ProjectPSX.Core {
    public class GPU {
        private uint GPUREAD;     //1F801810h-Read GPUREAD Receive responses to GP0(C0h) and GP1(10h) commands

        private uint command;
        private int commandSize;
        private uint[] commandBuffer = new uint[16];
        private int pointer;

        private int scanLine = 0;

        private static readonly int[] resolutions = { 256, 320, 512, 640, 368 };//gpustat res index
        private static readonly int[] dotClockDiv = { 10, 8, 5, 4, 7 };

        private IHostWindow window;

        private readonly IVRAM<ushort> VRAM16 = new VRAM16(1024, 512);

        private readonly IVRAM<int> VRAM32 = new VRAM32(1024, 512);

        public bool debug;

        private Mode mode;

        private VRamTransfer _vRamTransfer;


        private Point2D min = new Point2D();
        private Point2D   max = new Point2D();

        TextureData _textureData = new TextureData();

        private Color _color0;
        private Color _color1;
        private Color _color2;

        private bool isTextureDisabledAllowed;

        //GP0
        private int  MaskWhileDrawing;
        private bool CheckMaskBeforeDraw;
        private bool IsInterlaceField;
        private bool IsDisplayDisabled;
        private bool IsInterruptRequested;
        private bool IsDmaRequest;

        private bool IsReadyToReceiveCommand;
        private bool IsReadyToSendVRAMToCPU;
        private bool IsReadyToReceiveDmaBlock;

        private byte DmaDirection;
        private bool IsOddLine;

        #region GP0(E2h) - Texture Window setting

        private uint TextureWindowBits = 0xFFFF_FFFF;
        private int  TextureWindowPreMaskX;
        private int  TextureWindowPreMaskY;
        private int  TextureWindowPostMaskX;
        private int  TextureWindowPostMaskY;

        #endregion

        private ushort DisplayVRAMStartX;
        private ushort DisplayVRAMStartY;
        private ushort DisplayX1;
        private ushort DisplayX2;
        private ushort DisplayY1;
        private ushort DisplayY2;

        private int VideoCycles;
        private int TimingHorizontal = 3413;
        private int TimingVertical = 263;

        public GPU(IHostWindow window) {
            this.window = window;
            mode = Mode.COMMAND;
            GP1_00_ResetGPU();
            // TODO delete these once finished
            DrawMode      = new DrawMode();
            DrawingOffset = new DrawingOffset();
        }

        public bool tick(int cycles) {
            //Video clock is the cpu clock multiplied by 11/7.
            VideoCycles += cycles * 11 / 7;


            if (VideoCycles >= TimingHorizontal) {
                VideoCycles -= TimingHorizontal;
                scanLine++;

                if (!DisplayMode.IsVerticalResolution480) {
                    IsOddLine = (scanLine & 0x1) != 0;
                }

                if (scanLine >= TimingVertical) {
                    scanLine = 0;

                    if (DisplayMode.IsVerticalInterlace && DisplayMode.IsVerticalResolution480) {
                        IsOddLine = !IsOddLine;
                    }

                    window.Render(VRAM32.Pixels, VRAM16.Pixels);
                    return true;
                }
            }
            return false;
        }

        public (int dot, bool hblank, bool bBlank) getBlanksAndDot() { //test
            int dot = dotClockDiv[DisplayMode.HorizontalResolution2 << 2 | DisplayMode.HorizontalResolution1];
            bool hBlank = VideoCycles < DisplayX1 || VideoCycles > DisplayX2;
            bool vBlank = scanLine < DisplayY1 || scanLine > DisplayY2;

            return (dot, hBlank, vBlank);
        }

        public uint loadGPUSTAT() {
            uint GPUSTAT = 0;

            GPUSTAT |= DrawMode.TexturePageXBase;
            GPUSTAT |= (uint)DrawMode.TexturePageYBase << 4;
            GPUSTAT |= (uint)DrawMode.SemiTransparency << 5;
            GPUSTAT |= (uint)DrawMode.TexturePageColors << 7;
            GPUSTAT |= (uint)(DrawMode.Dither24BitTo15Bit ? 1 : 0) << 9;
            GPUSTAT |= (uint)(DrawMode.DrawingToDisplayArea ? 1 : 0) << 10;
            GPUSTAT |= (uint)MaskWhileDrawing << 11;
            GPUSTAT |= (uint)(CheckMaskBeforeDraw ? 1 : 0) << 12;
            GPUSTAT |= (uint)(IsInterlaceField ? 1 : 0) << 13;
            GPUSTAT |= (uint)(DisplayMode.IsReverseFlag ? 1 : 0) << 14;
            GPUSTAT |= (uint)(DrawMode.TextureDisable ? 1 : 0) << 15;
            GPUSTAT |= (uint)DisplayMode.HorizontalResolution2 << 16;
            GPUSTAT |= (uint)DisplayMode.HorizontalResolution1 << 17;
            GPUSTAT |= (uint)(DisplayMode.IsVerticalResolution480 ? 1 : 0);
            GPUSTAT |= (uint)(DisplayMode.IsPAL ? 1 : 0) << 20;
            GPUSTAT |= (uint)(DisplayMode.Is24BitDepth ? 1 : 0) << 21;
            GPUSTAT |= (uint)(DisplayMode.IsVerticalInterlace ? 1 : 0) << 22;
            GPUSTAT |= (uint)(IsDisplayDisabled ? 1 : 0) << 23;
            GPUSTAT |= (uint)(IsInterruptRequested ? 1 : 0) << 24;
            GPUSTAT |= (uint)(IsDmaRequest ? 1 : 0) << 25;

            GPUSTAT |= (uint)/*(isReadyToReceiveCommand ? 1 : 0)*/1 << 26;
            GPUSTAT |= (uint)/*(IsReadyToSendVRAMToCPU ? 1 : 0)*/1 << 27;
            GPUSTAT |= (uint)/*(isReadyToReceiveDMABlock ? 1 : 0)*/1 << 28;

            GPUSTAT |= (uint)DmaDirection << 29;
            GPUSTAT |= (uint)(IsOddLine ? 1 : 0) << 31;

            //Console.WriteLine("[GPU] LOAD GPUSTAT: {0}", GPUSTAT.ToString("x8"));
            return GPUSTAT;
        }

        public uint loadGPUREAD() {
            //TODO check if correct and refact
            uint value;
            if (_vRamTransfer.HalfWords > 0) {
                value = readFromVRAM();
            } else {
                value = GPUREAD;
            }
            //Console.WriteLine("[GPU] LOAD GPUREAD: {0}", value.ToString("x8"));
            return value;
        }

        public void write(uint addr, uint value) {
            uint register = addr & 0xF;
            if (register == 0) {
                writeGP0(value);
            } else if (register == 4) {
                writeGP1(value);
            } else {
                Console.WriteLine($"[GPU] Unhandled GPU write access to register {register} : {value}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void writeGP0(uint value) {
            //Console.WriteLine("Direct " + value.ToString("x8"));
            //Console.WriteLine(mode);
            if (mode == Mode.COMMAND) {
                DecodeGP0Command(value);
            } else {
                WriteToVRAM(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void processDma(Span<uint> dma) {
            if (mode == Mode.COMMAND) {
                DecodeGP0Command(dma);
            } else {
                for (int i = 0; i < dma.Length; i++) {
                    WriteToVRAM(dma[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteToVRAM(uint value) {
            ushort pixel1 = (ushort)(value >> 16);
            ushort pixel0 = (ushort)(value & 0xFFFF);

            pixel0 |= (ushort)(MaskWhileDrawing << 15);
            pixel1 |= (ushort)(MaskWhileDrawing << 15);

            drawVRAMPixel(pixel0);

            //Force exit if we arrived to the end pixel (fixes weird artifacts on textures on Metal Gear Solid)
            if (--_vRamTransfer.HalfWords == 0) {
                mode = Mode.COMMAND;
                return;
            }

            drawVRAMPixel(pixel1);

            if (--_vRamTransfer.HalfWords == 0) {
                mode = Mode.COMMAND;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint readFromVRAM() {
            ushort pixel0 = VRAM16.GetPixel(_vRamTransfer.X++ & 0x3FF, _vRamTransfer.Y & 0x1FF);
            ushort pixel1 = VRAM16.GetPixel(_vRamTransfer.X++ & 0x3FF, _vRamTransfer.Y & 0x1FF);
            if (_vRamTransfer.X == _vRamTransfer.OriginX + _vRamTransfer.W) {
                _vRamTransfer.X -= _vRamTransfer.W;
                _vRamTransfer.Y++;
            }
            _vRamTransfer.HalfWords -= 2;
            return (uint)(pixel1 << 16 | pixel0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void drawVRAMPixel(ushort val) {
            if (CheckMaskBeforeDraw) {
                int bg = VRAM32.GetPixel(_vRamTransfer.X, _vRamTransfer.Y);

                if (bg >> 24 == 0) {
                    int y1 = _vRamTransfer.Y & 0x1FF;
                    int color = color1555to8888(val);
                    VRAM32.SetPixel(_vRamTransfer.X & 0x3FF, y1, color);
                    int y = _vRamTransfer.Y & 0x1FF;
                    VRAM16.SetPixel(_vRamTransfer.X & 0x3FF, y, val);
                }
            } else {
                int y1 = _vRamTransfer.Y & 0x1FF;
                int color = color1555to8888(val);
                VRAM32.SetPixel(_vRamTransfer.X & 0x3FF, y1, color);
                int y = _vRamTransfer.Y & 0x1FF;
                VRAM16.SetPixel(_vRamTransfer.X & 0x3FF, y, val);
            }

            _vRamTransfer.X++;

            if (_vRamTransfer.X == _vRamTransfer.OriginX + _vRamTransfer.W) {
                _vRamTransfer.X -= _vRamTransfer.W;
                _vRamTransfer.Y++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecodeGP0Command(uint value) {
            if (pointer == 0) {
                command = value >> 24;
                commandSize = CommandSizeTable[(int)command];
                //Console.WriteLine("[GPU] Direct GP0 COMMAND: {0} size: {1}", value.ToString("x8"), commandSize);
            }

            commandBuffer[pointer++] = value;
            //Console.WriteLine("[GPU] Direct GP0: {0} buffer: {1}", value.ToString("x8"), pointer);

            if (pointer == commandSize || commandSize == 16 && (value & 0xF000_F000) == 0x5000_5000) {
                pointer = 0;
                //Console.WriteLine("EXECUTING");
                ExecuteGP0(command, commandBuffer.AsSpan());
                pointer = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecodeGP0Command(Span<uint> buffer) {
            //Console.WriteLine(commandBuffer.Length);

            while (pointer < buffer.Length) {
                if (mode == Mode.COMMAND) {
                    command = buffer[pointer] >> 24;
                    //if (debug) Console.WriteLine("Buffer Executing " + command.ToString("x2") + " pointer " + pointer);
                    ExecuteGP0(command, buffer);
                } else {
                    WriteToVRAM(buffer[pointer++]);
                }
            }
            pointer = 0;
            //Console.WriteLine("fin");
        }

        private void ExecuteGP0(uint opcode, Span<uint> buffer) {
            //Console.WriteLine("GP0 Command: " + opcode.ToString("x2"));
            switch (opcode) {
                case 0x00: GP0_00_NOP(); break;
                case 0x01: GP0_01_MemClearCache(); break;
                case 0x02: GP0_02_FillRectVRAM(buffer); break;
                case 0x1F: GP0_1F_InterruptRequest(); break;

                case 0xE1: GP0_E1_SetDrawMode(buffer[pointer++]); break;
                case 0xE2: GP0_E2_SetTextureWindow(buffer[pointer++]); break;
                case 0xE3: GP0_E3_SetDrawingAreaTopLeft(buffer[pointer++]); break;
                case 0xE4: GP0_E4_SetDrawingAreaBottomRight(buffer[pointer++]); break;
                case 0xE5: GP0_E5_SetDrawingOffset(buffer[pointer++]); break;
                case 0xE6: GP0_E6_SetMaskBit(buffer[pointer++]); break;

                case uint _ when opcode >= 0x20 && opcode <= 0x3F:
                    GP0_RenderPolygon(buffer); break;
                case uint _ when opcode >= 0x40 && opcode <= 0x5F:
                    GP0_RenderLine(buffer); break;
                case uint _ when opcode >= 0x60 && opcode <= 0x7F:
                    GP0_RenderRectangle(buffer); break;
                case uint _ when opcode >= 0x80 && opcode <= 0x9F:
                    GP0_MemCopyRectVRAMtoVRAM(buffer); break;
                case uint _ when opcode >= 0xA0 && opcode <= 0xBF:
                    GP0_MemCopyRectCPUtoVRAM(buffer); break;
                case uint _ when opcode >= 0xC0 && opcode <= 0xDF:
                    GP0_MemCopyRectVRAMtoCPU(buffer); break;
                case uint _ when (opcode >= 0x3 && opcode <= 0x1E) || opcode == 0xE0 || opcode >= 0xE7 && opcode <= 0xEF:
                    GP0_00_NOP(); break;

                default: Console.WriteLine("[GPU] Unsupported GP0 Command " + opcode.ToString("x8")); /*Console.ReadLine();*/ GP0_00_NOP(); break;
            }
        }

        private void rasterizeTri(Point2D v0, Point2D v1, Point2D v2, TextureData t0, TextureData t1, TextureData t2, uint c0, uint c1, uint c2, Primitive primitive) {

            int area = orient2d(v0, v1, v2);

            if (area == 0) return;

            if (area < 0) {
                (v1, v2) = (v2, v1);
                (t1, t2) = (t2, t1);
                (c1, c2) = (c2, c1);
                area = -area;
            }

            /*boundingBox*/
            int minX = Math.Min(v0.X, Math.Min(v1.X, v2.X));
            int minY = Math.Min(v0.Y, Math.Min(v1.Y, v2.Y));
            int maxX = Math.Max(v0.X, Math.Max(v1.X, v2.X));
            int maxY = Math.Max(v0.Y, Math.Max(v1.Y, v2.Y));

            if ((maxX - minX) > 1024 || (maxY - minY) > 512) return;

            /*clip*/
            min.X = (short)Math.Max(minX, DrawingAreaTopLeft.X);
            min.Y = (short)Math.Max(minY, DrawingAreaTopLeft.Y);
            max.X = (short)Math.Min(maxX, DrawingAreaBottomRight.X);
            max.Y = (short)Math.Min(maxY, DrawingAreaBottomRight.Y);

            int A01 = v0.Y - v1.Y, B01 = v1.X - v0.X;
            int A12 = v1.Y - v2.Y, B12 = v2.X - v1.X;
            int A20 = v2.Y - v0.Y, B20 = v0.X - v2.X;

            int bias0 = isTopLeft(v1, v2) ? 0 : -1;
            int bias1 = isTopLeft(v2, v0) ? 0 : -1;
            int bias2 = isTopLeft(v0, v1) ? 0 : -1;

            int w0_row = orient2d(v1, v2, min) + bias0;
            int w1_row = orient2d(v2, v0, min) + bias1;
            int w2_row = orient2d(v0, v1, min) + bias2;

            int baseColor = GetRgbColor(c0);

            // Rasterize
            for (int y = min.Y; y < max.Y; y++) {
                // Barycentric coordinates at start of row
                int w0 = w0_row;
                int w1 = w1_row;
                int w2 = w2_row;

                for (int x = min.X; x < max.X; x++) {
                    // If p is on or inside all edges, render pixel.
                    if ((w0 | w1 | w2) >= 0) {
                        //Adjustements per triangle instead of per pixel can be done at area level
                        //but it still does some little by 1 error apreciable on some textured quads
                        //I assume it could be handled recalculating AXX and BXX offsets but those maths are beyond my scope

                        //Check background mask
                        if (CheckMaskBeforeDraw) {
                            _color0.Value = (uint)VRAM32.GetPixel(x, y); //back
                            if (_color0.M != 0) {
                                w0 += A12;
                                w1 += A20;
                                w2 += A01;
                                continue;
                            }
                        }

                        // reset default color of the triangle calculated outside the for as it gets overwriten as follows...
                        int color = baseColor;

                        if (primitive.IsShaded) {
                            _color0.Value = c0;
                            _color1.Value = c1;
                            _color2.Value = c2;

                            int r = interpolate(w0 - bias0, w1 - bias1, w2 - bias2, _color0.R, _color1.R, _color2.R, area);
                            int g = interpolate(w0 - bias0, w1 - bias1, w2 - bias2, _color0.G, _color1.G, _color2.G, area);
                            int b = interpolate(w0 - bias0, w1 - bias1, w2 - bias2, _color0.B, _color1.B, _color2.B, area);
                            color = r << 16 | g << 8 | b;
                        }

                        if (primitive.IsTextured) {
                            int texelX = interpolate(w0 - bias0, w1 - bias1, w2 - bias2, t0.X, t1.X, t2.X, area);
                            int texelY = interpolate(w0 - bias0, w1 - bias1, w2 - bias2, t0.Y, t1.Y, t2.Y, area);
                            int texel = getTexel(maskTexelAxis(texelX, TextureWindowPreMaskX, TextureWindowPostMaskX), maskTexelAxis(texelY, TextureWindowPreMaskY, TextureWindowPostMaskY), primitive.Clut, primitive.TextureBase, primitive.Depth);
                            if (texel == 0) {
                                w0 += A12;
                                w1 += A20;
                                w2 += A01;
                                continue;
                            }

                            if (!primitive.IsRawTextured) {
                                _color0.Value = (uint)color;
                                _color1.Value = (uint)texel;
                                _color1.R = clampToFF(_color0.R * _color1.R >> 7);
                                _color1.G = clampToFF(_color0.G * _color1.G >> 7);
                                _color1.B = clampToFF(_color0.B * _color1.B >> 7);

                                texel = (int)_color1.Value;
                            }

                            color = texel;
                        }

                        if (primitive.IsSemiTransparent && (!primitive.IsTextured || (color & 0xFF00_0000) != 0)) {
                            color = handleSemiTransp(x, y, color, primitive.SemiTransparencyMode);
                        }

                        color |= MaskWhileDrawing << 24;

                        VRAM32.SetPixel(x, y, color);

                        ushort color3 = (ushort)Rgb888ToRgb555(color);
                        VRAM16.SetPixel(x, y, color3);
                    }
                    // One step to the right
                    w0 += A12;
                    w1 += A20;
                    w2 += A01;
                }
                // One row step
                w0_row += B12;
                w1_row += B20;
                w2_row += B01;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short Rgb888ToRgb555(int color)
        {
            var r = ((color >> 00) & 0xFF) * 31 / 255;
            var g = ((color >> 08) & 0xFF) * 31 / 255;
            var b = ((color >> 16) & 0xFF) * 31 / 255;

            return (short)((r << 10) | (g << 5) | b);
        }

        private void rasterizeLine(uint v1, uint v2, uint color1, uint color2, bool isTransparent) {
            short x = Read11BitShort(v1 & 0xFFFF);
            short y = Read11BitShort(v1 >> 16);

            short x2 = Read11BitShort(v2 & 0xFFFF);
            short y2 = Read11BitShort(v2 >> 16);

            if (Math.Abs(x - x2) > 0x3FF || Math.Abs(y - y2) > 0x1FF) return;

            x += DrawingOffset.X;
            y += DrawingOffset.Y;

            x2 += DrawingOffset.X;
            y2 += DrawingOffset.Y;

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);

            if (!(longest > shortest)) {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest >> 1;

            for (int i = 0; i <= longest; i++) {
                float ratio = (float)i / longest;
                int color = interpolate(color1, color2, ratio);

                //x = (short)Math.Min(Math.Max(x, drawingAreaLeft), drawingAreaRight); //this generates glitches on RR4
                //y = (short)Math.Min(Math.Max(y, drawingAreaTop), drawingAreaBottom);

                if (x >= DrawingAreaTopLeft.X && x < DrawingAreaBottomRight.X && y >= DrawingAreaTopLeft.Y && y < DrawingAreaBottomRight.Y) {
                    //if (primitive.isSemiTransparent && (!primitive.isTextured || (color & 0xFF00_0000) != 0)) {
                    if (isTransparent) {
                        color = handleSemiTransp(x, y, color, DrawMode.SemiTransparency);
                    }

                    color |= MaskWhileDrawing << 24;

                    VRAM32.SetPixel(x, y, color);
                    ushort color3 = (ushort)Rgb888ToRgb555(color);
                    VRAM16.SetPixel(x, y, color3);
                }

                numerator += shortest;
                if (!(numerator < longest)) {
                    numerator -= longest;
                    x += (short)dx1;
                    y += (short)dy1;
                } else {
                    x += (short)dx2;
                    y += (short)dy2;
                }
            }
            //Console.ReadLine();
        }

        private void rasterizeRect(Point2D origin, Point2D size, TextureData texture, uint bgrColor, Primitive primitive) {
            int xOrigin = Math.Max(origin.X, DrawingAreaTopLeft.X);
            int yOrigin = Math.Max(origin.Y, DrawingAreaTopLeft.Y);
            int width = Math.Min(size.X,  DrawingAreaBottomRight.X);
            int height = Math.Min(size.Y, DrawingAreaBottomRight.Y);

            int uOrigin = texture.X + (xOrigin - origin.X);
            int vOrigin = texture.Y + (yOrigin - origin.Y);

            int baseColor = GetRgbColor(bgrColor);

            for (int y = yOrigin, v = vOrigin; y < height; y++, v++) {
                for (int x = xOrigin, u = uOrigin; x < width; x++, u++) {
                    //Check background mask
                    if (CheckMaskBeforeDraw) {
                        int y1 = y & 0x1FF;
                        _color0.Value = (uint)VRAM32.GetPixel(x & 0x3FF, y1); //back
                        if (_color0.M != 0) continue;
                    }

                    int color = baseColor;

                    if (primitive.IsTextured) {
                        //int texel = getTexel(u, v, clut, textureBase, depth);
                        int texel = getTexel(maskTexelAxis(u, TextureWindowPreMaskX, TextureWindowPostMaskX),maskTexelAxis(v, TextureWindowPreMaskY, TextureWindowPostMaskY),primitive.Clut, primitive.TextureBase, primitive.Depth);
                        if (texel == 0) {
                            continue;
                        }

                        if (!primitive.IsRawTextured) {
                            _color0.Value = (uint)color;
                            _color1.Value = (uint)texel;
                            _color1.R = clampToFF(_color0.R * _color1.R >> 7);
                            _color1.G = clampToFF(_color0.G * _color1.G >> 7);
                            _color1.B = clampToFF(_color0.B * _color1.B >> 7);

                            texel = (int)_color1.Value;
                        }

                        color = texel;
                    }

                    if (primitive.IsSemiTransparent && (!primitive.IsTextured || (color & 0xFF00_0000) != 0)) {
                        color = handleSemiTransp(x, y, color, primitive.SemiTransparencyMode);
                    }

                    color |= MaskWhileDrawing << 24;

                    VRAM32.SetPixel(x, y, color);
                    ushort color3 = (ushort)Rgb888ToRgb555(color);
                    VRAM16.SetPixel(x, y, color3);
                }

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int maskTexelAxis(int axis, int preMaskAxis, int postMaskAxis) {
            return axis & 0xFF & preMaskAxis | postMaskAxis;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int getTexel(int x, int y, Point2D clut, Point2D textureBase, int depth) {
            if (depth == 0) {
                return get4bppTexel(x, y, clut, textureBase);
            } else if (depth == 1) {
                return get8bppTexel(x, y, clut, textureBase);
            } else {
                return get16bppTexel(x, y, textureBase);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int get4bppTexel(int x, int y, Point2D clut, Point2D textureBase) {
            int y1 = y + textureBase.Y;
            ushort index = VRAM16.GetPixel(x / 4 + textureBase.X, y1);
            int p = (index >> (x & 3) * 4) & 0xF;
            return VRAM32.GetPixel(clut.X + p, clut.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int get8bppTexel(int x, int y, Point2D clut, Point2D textureBase) {
            int y1 = y + textureBase.Y;
            ushort index = VRAM16.GetPixel(x / 2 + textureBase.X, y1);
            int p = (index >> (x & 1) * 8) & 0xFF;
            return VRAM32.GetPixel(clut.X + p, clut.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int get16bppTexel(int x, int y, Point2D textureBase)
        {
            int y1 = y + textureBase.Y;
            return VRAM32.GetPixel(x + textureBase.X, y1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int orient2d(Point2D a, Point2D b, Point2D c) {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        public void writeGP1(uint value) {
            //Console.WriteLine($"[GPU] GP1 Write Value: {value:x8}");
            uint opcode = value >> 24;
            switch (opcode) {
                case 0x00: GP1_00_ResetGPU(); break;
                case 0x01: GP1_01_ResetCommandBuffer(); break;
                case 0x02: GP1_02_AckGPUInterrupt(); break;
                case 0x03: GP1_03_DisplayEnable(value); break;
                case 0x04: GP1_04_DMADirection(value); break;
                case 0x05: GP1_05_DisplayVRAMStart(value); break;
                case 0x06: GP1_06_DisplayHorizontalRange(value); break;
                case 0x07: GP1_07_DisplayVerticalRange(value); break;
                case 0x08: GP1_08_DisplayMode(value); break;
                case 0x09: GP1_09_TextureDisable(value); break;
                case uint _ when opcode >= 0x10 && opcode <= 0x1F:
                    GP1_GPUInfo(value); break;
                default: Console.WriteLine("[GPU] Unsupported GP1 Command " + opcode.ToString("x8")); Console.ReadLine(); break;
            }
        }

        private          int      lastHr;
        private          int      lastVr;
        private          bool     last24;

        private DrawMode      DrawMode;
        private DrawingOffset DrawingOffset;
        private DrawingArea   DrawingAreaTopLeft;
        private DrawingArea   DrawingAreaBottomRight;

        private  DisplayMode DisplayMode { get; set; }

        private uint getTexpageFromGPU() {
            uint texpage = 0;

            texpage |= (DrawMode.TexturedRectangleYFlip ? 1u : 0) << 13;
            texpage |= (DrawMode.TexturedRectangleXFlip ? 1u : 0) << 12;
            texpage |= (DrawMode.TextureDisable ? 1u : 0) << 11;
            texpage |= (DrawMode.DrawingToDisplayArea ? 1u : 0) << 10;
            texpage |= (DrawMode.Dither24BitTo15Bit ? 1u : 0) << 9;
            texpage |= (uint)(DrawMode.TexturePageColors << 7);
            texpage |= (uint)(DrawMode.SemiTransparency << 5);
            texpage |= (uint)(DrawMode.TexturePageYBase << 4);
            texpage |= DrawMode.TexturePageXBase;

            return texpage;
        }

        private int handleSemiTransp(int x, int y, int color, int semiTranspMode) {
            _color0.Value = (uint)VRAM32.GetPixel(x, y); //back
            _color1.Value = (uint)color; //front
            switch (semiTranspMode) {
                case 0: //0.5 x B + 0.5 x F    ;aka B/2+F/2
                    _color1.R = (byte)((_color0.R + _color1.R) >> 1);
                    _color1.G = (byte)((_color0.G + _color1.G) >> 1);
                    _color1.B = (byte)((_color0.B + _color1.B) >> 1);
                    break;
                case 1://1.0 x B + 1.0 x F    ;aka B+F
                    _color1.R = clampToFF(_color0.R + _color1.R);
                    _color1.G = clampToFF(_color0.G + _color1.G);
                    _color1.B = clampToFF(_color0.B + _color1.B);
                    break;
                case 2: //1.0 x B - 1.0 x F    ;aka B-F
                    _color1.R = clampToZero(_color0.R - _color1.R);
                    _color1.G = clampToZero(_color0.G - _color1.G);
                    _color1.B = clampToZero(_color0.B - _color1.B);
                    break;
                case 3: //1.0 x B +0.25 x F    ;aka B+F/4
                    _color1.R = clampToFF(_color0.R + (_color1.R >> 2));
                    _color1.G = clampToFF(_color0.G + (_color1.G >> 2));
                    _color1.B = clampToFF(_color0.B + (_color1.B >> 2));
                    break;
            }//actually doing RGB calcs on BGR struct...
            return (int)_color1.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte clampToZero(int v) {
            if (v < 0) return 0;
            else return (byte)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte clampToFF(int v) {
            if (v > 0xFF) return 0xFF;
            else return (byte)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetRgbColor(uint value) {
            _color0.Value = value;
            return (_color0.M << 24 | _color0.R << 16 | _color0.G << 8 | _color0.B);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool isTopLeft(Point2D a, Point2D b) => a.Y == b.Y && b.X > a.X || b.Y < a.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int interpolate(uint c1, uint c2, float ratio) {
            _color1.Value = c1;
            _color2.Value = c2;

            byte r = (byte)(_color2.R * ratio + _color1.R * (1 - ratio));
            byte g = (byte)(_color2.G * ratio + _color1.G * (1 - ratio));
            byte b = (byte)(_color2.B * ratio + _color1.B * (1 - ratio));

            return (r << 16 | g << 8 | b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int interpolate(int w0, int w1, int w2, int t0, int t1, int t2, int area) {
            //https://codeplea.com/triangular-interpolation
            return (t0 * w0 + t1 * w1 + t2 * w2) / area;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Read11BitShort(uint value)
        {
            return (short)(((int)value << 21) >> 21);
        }

        //This needs to go away once a BGR bitmap is achieved
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int color1555to8888(ushort val) {
            byte m = (byte)(val >> 15);
            byte r = (byte)((val & 0x1F) << 3);
            byte g = (byte)(((val >> 5) & 0x1F) << 3);
            byte b = (byte)(((val >> 10) & 0x1F) << 3);

            return (m << 24 | r << 16 | g << 8 | b);
        }

        //This is only needed for the Direct GP0 commands as the command number needs to be
        //known ahead of the first command on queue.
        private static ReadOnlySpan<byte> CommandSizeTable => new byte[] {
            //0  1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
             1,  1,  3,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, //0
             1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, //1
             4,  4,  4,  4,  7,  7,  7,  7,  5,  5,  5,  5,  9,  9,  9,  9, //2
             6,  6,  6,  6,  9,  9,  9,  9,  8,  8,  8,  8, 12, 12, 12, 12, //3
             3,  3,  3,  3,  3,  3,  3,  3, 16, 16, 16, 16, 16, 16, 16, 16, //4
             4,  4,  4,  4,  4,  4,  4,  4, 16, 16, 16, 16, 16, 16, 16, 16, //5
             3,  3,  3,  1,  4,  4,  4,  4,  2,  1,  2,  1,  3,  3,  3,  3, //6
             2,  1,  2,  1,  3,  3,  3,  3,  2,  1,  2,  2,  3,  3,  3,  3, //7
             4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4, //8
             4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4, //9
             3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3, //A
             3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3, //B
             3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3, //C
             3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3,  3, //D
             1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, //E
             1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1  //F
        };

        #region GP0 Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GP0_00_NOP()
        {
            pointer++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GP0_01_MemClearCache()
        {
            pointer++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GP0_02_FillRectVRAM(Span<uint> buffer)
            // GP0(02h) - Fill Rectangle in VRAM
        {
            _color0.Value = buffer[pointer++];

            var yx = buffer[pointer++];
            var hw = buffer[pointer++];

            var x = (ushort)(yx & 0x3F0);
            var y = (ushort)((yx >> 16) & 0x1FF);

            var w = (ushort)(((hw & 0x3FF) + 0xF) & ~0xF);
            var h = (ushort)((hw >> 16) & 0x1FF);

            var bgr555 = (ushort)(((_color0.B * 31 / 255) << 10) | ((_color0.G * 31 / 255) << 5) | ((_color0.R * 31 / 255) << 0));
            var rgb888 = (_color0.R << 16) | (_color0.G << 8) | _color0.B;

            if (x + w <= 0x3FF && y + h <= 0x1FF)
            {
                var span16 = new Span<ushort>(VRAM16.Pixels);
                var span24 = new Span<int>(VRAM32.Pixels);

                for (int yPos = y; yPos < h + y; yPos++)
                {
                    var start = yPos * 1024 + x;
                    span16.Slice(start, w).Fill(bgr555);
                    span24.Slice(start, w).Fill(rgb888);
                }
            }
            else
            {
                for (int yPos = y; yPos < h + y; yPos++)
                {
                    for (int xPos = x; xPos < w + x; xPos++)
                    {
                        var y2 = yPos & 0x1FF;
                        VRAM32.SetPixel(xPos & 0x3FF, y2, rgb888);
                        var y1 = yPos & 0x1FF;
                        VRAM16.SetPixel(xPos & 0x3FF, y1, bgr555);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GP0_1F_InterruptRequest()
        {
            pointer++;
            IsInterruptRequested = true;
        }

        private void GP0_E1_SetDrawMode(uint val)
        {
            DrawMode.TexturePageXBase       = (byte)(val & 0xF);
            DrawMode.TexturePageYBase       = (byte)((val >> 4) & 0x1);
            DrawMode.SemiTransparency       = (byte)((val >> 5) & 0x3);
            DrawMode.TexturePageColors      = (byte)((val >> 7) & 0x3);
            DrawMode.Dither24BitTo15Bit     = ((val >> 9) & 0x1) != 0;
            DrawMode.DrawingToDisplayArea   = ((val >> 10) & 0x1) != 0;
            DrawMode.TextureDisable         = isTextureDisabledAllowed && ((val >> 11) & 0x1) != 0;
            DrawMode.TexturedRectangleXFlip = ((val >> 12) & 0x1) != 0;
            DrawMode.TexturedRectangleYFlip = ((val >> 13) & 0x1) != 0;

            //Console.WriteLine("[GPU] [GP0] DrawMode ");
        }

        private void GP0_E2_SetTextureWindow(uint value)
        {
            var bits = value & 0xFF_FFFF;
            if (bits == TextureWindowBits)
                return;

            TextureWindowBits = bits;

            var textureWindow = new TextureWindow(value);

            TextureWindowPreMaskX  = ~(textureWindow.MaskX * 8);
            TextureWindowPreMaskY  = ~(textureWindow.MaskY * 8);
            TextureWindowPostMaskX = (textureWindow.OffsetX & textureWindow.MaskX) * 8;
            TextureWindowPostMaskY = (textureWindow.OffsetY & textureWindow.MaskY) * 8;
        }

        private void GP0_E3_SetDrawingAreaTopLeft(uint value)
        {
            DrawingAreaTopLeft = new DrawingArea(value);
        }

        private void GP0_E4_SetDrawingAreaBottomRight(uint value)
        {
            DrawingAreaBottomRight = new DrawingArea(value);
        }

        private void GP0_E5_SetDrawingOffset(uint val)
        {
            DrawingOffset = new DrawingOffset(val);
        }

        private void GP0_E6_SetMaskBit(uint val)
        {
            MaskWhileDrawing    = (int)(val & 0x1);
            CheckMaskBeforeDraw = (val & 0x2) != 0;
        }

        private void GP0_RenderLine(Span<uint> buffer)
        {
            //Console.WriteLine("size " + commandBuffer.Count);
            //int arguments = 0;
            var command = buffer[pointer++];
            //arguments++;

            var color1 = command & 0xFFFFFF;
            var color2 = color1;

            var isPoly = (command & (1 << 27)) != 0;
            var isShaded = (command & (1 << 28)) != 0;
            var isTransparent = (command & (1 << 25)) != 0;

            //if (isTextureMapped /*isRaw*/) return;

            var v1 = buffer[pointer++];
            //arguments++;

            if (isShaded)
            {
                color2 = buffer[pointer++];
                //arguments++;
            }

            var v2 = buffer[pointer++];
            //arguments++;

            rasterizeLine(v1, v2, color1, color2, isTransparent);

            if (!isPoly) return;
            //renderline = 0;
            while ( /*arguments < 0xF &&*/ (buffer[pointer] & 0xF000_F000) != 0x5000_5000)
            {
                //Console.WriteLine("DOING ANOTHER LINE " + ++renderline);
                //arguments++;
                color1 = color2;
                if (isShaded)
                {
                    color2 = buffer[pointer++];
                    //arguments++;
                }

                v1 = v2;
                v2 = buffer[pointer++];
                rasterizeLine(v1, v2, color1, color2, isTransparent);
                //Console.WriteLine("RASTERIZE " + ++rasterizeline);
                //window.update(VRAM32.Bits);
                //Console.ReadLine();
            }

            /*if (arguments != 0xF) */
            pointer++; // discard 5555_5555 termination (need to rewrite all this from the GP0...)
        }

        public void GP0_RenderPolygon(Span<uint> buffer)
        {
            var command = buffer[pointer];
            //Console.WriteLine(command.ToString("x8") +  " "  + commandBuffer.Length + " " + pointer);

            var isQuad = (command & (1 << 27)) != 0;

            var isShaded = (command & (1 << 28)) != 0;
            var isTextured = (command & (1 << 26)) != 0;
            var isSemiTransparent = (command & (1 << 25)) != 0;
            var isRawTextured = (command & (1 << 24)) != 0;

            var primitive = new Primitive();
            primitive.IsShaded          = isShaded;
            primitive.IsTextured        = isTextured;
            primitive.IsSemiTransparent = isSemiTransparent;
            primitive.IsRawTextured     = isRawTextured;

            var vertexN = isQuad ? 4 : 3;
            Span<uint> c = stackalloc uint[vertexN];
            Span<Point2D> v = stackalloc Point2D[vertexN];
            Span<TextureData> t = stackalloc TextureData[vertexN];

            if (!isShaded)
            {
                var color = buffer[pointer++];
                c[0] = color; //triangle 1 opaque color
                c[1] = color; //triangle 2 opaque color
            }

            primitive.SemiTransparencyMode = DrawMode.SemiTransparency;

            for (var i = 0; i < vertexN; i++)
            {
                if (isShaded) c[i] = buffer[pointer++];

                var xy = buffer[pointer++];
                v[i].X = (short)(Read11BitShort(xy & 0xFFFF) + DrawingOffset.X);
                v[i].Y = (short)(Read11BitShort(xy >> 16) + DrawingOffset.Y);

                if (isTextured)
                {
                    var textureData = buffer[pointer++];
                    t[i].Value = (ushort)textureData;
                    if (i == 0)
                    {
                        var palette = textureData >> 16;

                        primitive.Clut.X = (short)((palette & 0x3f) << 4);
                        primitive.Clut.Y = (short)((palette >> 6) & 0x1FF);
                    }
                    else if (i == 1)
                    {
                        var texpage = textureData >> 16;

                        //SET GLOBAL GPU E1
                        DrawMode.TexturePageXBase  = (byte)(texpage & 0xF);
                        DrawMode.TexturePageYBase  = (byte)((texpage >> 4) & 0x1);
                        DrawMode.SemiTransparency  = (byte)((texpage >> 5) & 0x3);
                        DrawMode.TexturePageColors = (byte)((texpage >> 7) & 0x3);
                        DrawMode.TextureDisable    = isTextureDisabledAllowed && ((texpage >> 11) & 0x1) != 0;

                        primitive.Depth                = DrawMode.TexturePageColors;
                        primitive.TextureBase.X        = (short)(DrawMode.TexturePageXBase << 6);
                        primitive.TextureBase.Y        = (short)(DrawMode.TexturePageYBase << 8);
                        primitive.SemiTransparencyMode = DrawMode.SemiTransparency;
                    }
                }
            }

            rasterizeTri(v[0], v[1], v[2], t[0], t[1], t[2], c[0], c[1], c[2], primitive);
            if (isQuad) rasterizeTri(v[1], v[2], v[3], t[1], t[2], t[3], c[1], c[2], c[3], primitive);
        }

        private void GP0_RenderRectangle(Span<uint> buffer)
        {
            //1st Color+Command(CcBbGgRrh)
            //2nd Vertex(YyyyXxxxh)
            //3rd Texcoord+Palette(ClutYyXxh)(for 4bpp Textures Xxh must be even!) //Only textured
            //4rd (3rd non textured) Width + Height(YsizXsizh)(variable opcode only)(max 1023x511)
            var command = buffer[pointer++];
            var color = command & 0xFFFFFF;
            var opcode = command >> 24;

            var isTextured = (command & (1 << 26)) != 0;
            var isSemiTransparent = (command & (1 << 25)) != 0;
            var isRawTextured = (command & (1 << 24)) != 0;

            var primitive = new Primitive();
            primitive.IsTextured        = isTextured;
            primitive.IsSemiTransparent = isSemiTransparent;
            primitive.IsRawTextured     = isRawTextured;

            var vertex = buffer[pointer++];
            var xo = (short)(vertex & 0xFFFF);
            var yo = (short)(vertex >> 16);

            if (isTextured)
            {
                var texture = buffer[pointer++];
                _textureData.X = (byte)(texture & 0xFF);
                _textureData.Y = (byte)((texture >> 8) & 0xFF);

                var palette = (ushort)((texture >> 16) & 0xFFFF);
                primitive.Clut.X = (short)((palette & 0x3f) << 4);
                primitive.Clut.Y = (short)((palette >> 6) & 0x1FF);
            }

            primitive.Depth                = DrawMode.TexturePageColors;
            primitive.TextureBase.X        = (short)(DrawMode.TexturePageXBase << 6);
            primitive.TextureBase.Y        = (short)(DrawMode.TexturePageYBase << 8);
            primitive.SemiTransparencyMode = DrawMode.SemiTransparency;

            short width = 0;
            short heigth = 0;

            switch ((opcode & 0x18) >> 3)
            {
                case 0x0:
                    var hw = buffer[pointer++];
                    width  = (short)(hw & 0xFFFF);
                    heigth = (short)(hw >> 16);
                    break;
                case 0x1:
                    width  = 1;
                    heigth = 1;
                    break;
                case 0x2:
                    width  = 8;
                    heigth = 8;
                    break;
                case 0x3:
                    width  = 16;
                    heigth = 16;
                    break;
            }

            var y = Read11BitShort((uint)(yo + DrawingOffset.Y));
            var x = Read11BitShort((uint)(xo + DrawingOffset.X));

            Point2D origin;
            origin.X = x;
            origin.Y = y;

            Point2D size;
            size.X = (short)(x + width);
            size.Y = (short)(y + heigth);

            rasterizeRect(origin, size, _textureData, color, primitive);
        }

        private void GP0_MemCopyRectVRAMtoVRAM(Span<uint> buffer)
        {
            pointer++; //Command/Color parameter unused
            var sourceXY = buffer[pointer++];
            var destinationXY = buffer[pointer++];
            var wh = buffer[pointer++];

            var sx = (ushort)(sourceXY & 0x3FF);
            var sy = (ushort)((sourceXY >> 16) & 0x1FF);

            var dx = (ushort)(destinationXY & 0x3FF);
            var dy = (ushort)((destinationXY >> 16) & 0x1FF);

            var w = (ushort)((((wh & 0xFFFF) - 1) & 0x3FF) + 1);
            var h = (ushort)((((wh >> 16) - 1) & 0x1FF) + 1);

            for (var yPos = 0; yPos < h; yPos++)
            {
                for (var xPos = 0; xPos < w; xPos++)
                {
                    var y1 = (sy + yPos) & 0x1FF;
                    var rgb888 = VRAM32.GetPixel((sx + xPos) & 0x3FF, y1);
                    var bgr555 = VRAM16.GetPixel((sx + xPos) & 0x3FF, (sy + yPos) & 0x1FF);

                    if (CheckMaskBeforeDraw)
                    {
                        var y2 = (dy + yPos) & 0x1FF;
                        _color0.Value = (uint)VRAM32.GetPixel((dx + xPos) & 0x3FF, y2);
                        if (_color0.M != 0) continue;
                    }

                    rgb888 |= MaskWhileDrawing << 24;

                    var y3 = (dy + yPos) & 0x1FF;
                    VRAM32.SetPixel((dx + xPos) & 0x3FF, y3, rgb888);
                    var y = (dy + yPos) & 0x1FF;
                    VRAM16.SetPixel((dx + xPos) & 0x3FF, y, bgr555);
                }
            }
        }

        private void GP0_MemCopyRectCPUtoVRAM(Span<uint> buffer)
        {
            //todo rewrite VRAM coord struct mess
            pointer++; //Command/Color parameter unused
            var yx = buffer[pointer++];
            var wh = buffer[pointer++];

            var x = (ushort)(yx & 0x3FF);
            var y = (ushort)((yx >> 16) & 0x1FF);

            var w = (ushort)((((wh & 0xFFFF) - 1) & 0x3FF) + 1);
            var h = (ushort)((((wh >> 16) - 1) & 0x1FF) + 1);

            _vRamTransfer.X         = x;
            _vRamTransfer.Y         = y;
            _vRamTransfer.W         = w;
            _vRamTransfer.H         = h;
            _vRamTransfer.OriginX   = x;
            _vRamTransfer.OriginY   = y;
            _vRamTransfer.HalfWords = w * h;

            mode = Mode.VRAM;
        }

        private void GP0_MemCopyRectVRAMtoCPU(Span<uint> buffer)
        {
            pointer++; //Command/Color parameter unused
            var yx = buffer[pointer++];
            var wh = buffer[pointer++];

            var x = (ushort)(yx & 0x3FF);
            var y = (ushort)((yx >> 16) & 0x1FF);

            var w = (ushort)((((wh & 0xFFFF) - 1) & 0x3FF) + 1);
            var h = (ushort)((((wh >> 16) - 1) & 0x1FF) + 1);

            _vRamTransfer.X         = x;
            _vRamTransfer.Y         = y;
            _vRamTransfer.W         = w;
            _vRamTransfer.H         = h;
            _vRamTransfer.OriginX   = x;
            _vRamTransfer.OriginY   = y;
            _vRamTransfer.HalfWords = w * h;
        }

        #endregion

        #region GP1 Methods

        private void GP1_00_ResetGPU()
        {
            GP1_01_ResetCommandBuffer();
            GP1_02_AckGPUInterrupt();
            GP1_03_DisplayEnable(1);
            GP1_04_DMADirection(0);
            GP1_05_DisplayVRAMStart(0);
            GP1_06_DisplayHorizontalRange(0xC00200);
            GP1_07_DisplayVerticalRange(0x100010);
            GP1_08_DisplayMode(0);

            GP0_E1_SetDrawMode(0);
            GP0_E2_SetTextureWindow(0);
            GP0_E3_SetDrawingAreaTopLeft(0);
            GP0_E4_SetDrawingAreaBottomRight(0);
            GP0_E5_SetDrawingOffset(0);
            GP0_E6_SetMaskBit(0);
        }

        private void GP1_01_ResetCommandBuffer()
        {
            pointer = 0;
        }

        private void GP1_02_AckGPUInterrupt()
        {
            IsInterruptRequested = false;
        }

        private void GP1_03_DisplayEnable(uint value)
        {
            IsDisplayDisabled = (value & 1) != 0;
        }

        private void GP1_04_DMADirection(uint value)
        {
            DmaDirection = (byte)(value & 0x3);
        }

        private void GP1_05_DisplayVRAMStart(uint value)
        {
            DisplayVRAMStartX = (ushort)(value & 0x3FE);
            DisplayVRAMStartY = (ushort)((value >> 10) & 0x1FE);

            Console.WriteLine(
                $"[GPU] {nameof(GP1_05_DisplayVRAMStart)}: {nameof(DisplayVRAMStartX)} = {DisplayVRAMStartX}, {nameof(DisplayVRAMStartY)} = {DisplayVRAMStartY}");

            window.SetVRAMStart(DisplayVRAMStartX, DisplayVRAMStartY);
        }

        private void GP1_06_DisplayHorizontalRange(uint value)
        {
            DisplayX1 = (ushort)(value & 0xFFF);
            DisplayX2 = (ushort)((value >> 12) & 0xFFF);

            Console.WriteLine(
                $"[GPU] {nameof(GP1_06_DisplayHorizontalRange)}: {nameof(DisplayX1)} = {DisplayX1}, {nameof(DisplayX2)} = {DisplayX2}");

            window.SetHorizontalRange(DisplayX1, DisplayX2);
        }

        private void GP1_07_DisplayVerticalRange(uint value)
        {
            DisplayY1 = (ushort)(value & 0x3FF);
            DisplayY2 = (ushort)((value >> 10) & 0x3FF);

            Console.WriteLine(
                $"[GPU] {nameof(GP1_07_DisplayVerticalRange)}: {nameof(DisplayY1)} = {DisplayY1}, {nameof(DisplayY2)} = {DisplayY2}");

            window.SetVerticalRange(DisplayY1, DisplayY2);
        }

        private void GP1_08_DisplayMode(uint value)
        {
            DisplayMode = new DisplayMode(value);

            IsInterlaceField = DisplayMode.IsVerticalInterlace;

            TimingHorizontal = DisplayMode.IsPAL ? 3406 : 3413;
            TimingVertical   = DisplayMode.IsPAL ? 314 : 263;

            var horizontalRes = resolutions[(DisplayMode.HorizontalResolution2 << 2) | DisplayMode.HorizontalResolution1];
            var verticalRes = DisplayMode.IsVerticalResolution480 ? 480 : 240;


            //if (lastHr == horizontalRes && lastVr == verticalRes && last24 == is24BitDepth) 
            //    return;

            Console.WriteLine(
                $"[GPU] {nameof(GP1_08_DisplayMode)}: {nameof(horizontalRes)} = {horizontalRes}, {nameof(verticalRes)} = {verticalRes}, {nameof(DisplayMode.Is24BitDepth)} = {DisplayMode.Is24BitDepth}");

            window.SetDisplayMode(horizontalRes, verticalRes, DisplayMode.Is24BitDepth);

            lastHr = horizontalRes;
            lastVr = verticalRes;
            last24 = DisplayMode.Is24BitDepth;
        }

        private void GP1_09_TextureDisable(uint value)
        {
            isTextureDisabledAllowed = (value & 0x1) != 0;
        }

        private void GP1_GPUInfo(uint value)
        {
            var info = value & 0xF;

            switch (info)
            {
                case 0x2:
                    GPUREAD = TextureWindowBits;
                    break;
                case 0x3:
                    GPUREAD = (uint)((DrawingAreaTopLeft.Y << 10) | DrawingAreaTopLeft.X);
                    break;
                case 0x4:
                    GPUREAD = (uint)((DrawingAreaBottomRight.Y << 10) | DrawingAreaBottomRight.X);
                    break;
                case 0x5:
                    GPUREAD = (uint)((DrawingOffset.Y << 11) | (ushort)DrawingOffset.X);
                    break;
                case 0x7:
                    GPUREAD = 2;
                    break;
                case 0x8:
                    GPUREAD = 0;
                    break;
                default:
                    Console.WriteLine($"[GPU] GP1 Unhandled GetInfo: 0x{info:x8}");
                    break;
            }
        }

        #endregion
    }
}