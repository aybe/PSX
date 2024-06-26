﻿using System;
using System.IO;
using PSX.Core;
using PSX.Core.Graphics;
using PSX.Core.Interfaces;
using PSX.Core.Processor;
using PSX.Core.Sound;
using PSX.Devices.Input;
using PSX.Devices.Optical;
using PSX.Devices.Storage;

namespace PSX;

public sealed class Emulator : IDisposable
{
    private const int PSX_MHZ          = 33868800;
    private const int SYNC_CYCLES      = 100;
    private const int MIPS_UNDERCLOCK  = 3; // Testing: This compensates the absence of HALT instruction on MIPS Architecture, may broke some games.
    private const int CYCLES_PER_FRAME = PSX_MHZ / 60;
    private const int SYNC_LOOPS       = CYCLES_PER_FRAME / (SYNC_CYCLES * MIPS_UNDERCLOCK) + 1;

    private readonly JOYPAD Joypad;

    public Emulator(IHostWindow window, string path, IController? controller1 = null, IController? controller2 = null)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

        controller1 ??= new NullController(new NullControllerSource());
        controller2 ??= new NullController(new NullControllerSource());

        var card          = new MemoryCard();
        var irqController = new InterruptController();
        var cd            = new CD(path);
        var spu           = new SPU(window, irqController, new Sector(Sector.XA_BUFFER));

        Joypad = new JOYPAD(controller1, controller2, card);

        var timers = new TIMERS();
        var mdec   = new MDEC();

        Cdrom = new CDROM(cd, spu);
        Gpu   = new GPU(window);
        Bus   = new BUS(Gpu, Cdrom, spu, Joypad, timers, mdec, irqController);
        Cpu   = new CPU(Bus);

        Bus.loadBios();

        if (Path.GetExtension(path).ToUpperInvariant() is ".EXE")
        {
            Bus.loadEXE(path);
        }
    }

    private BUS Bus { get; }

    private CPU Cpu { get; }

    private GPU Gpu { get; }

    private CDROM Cdrom { get; }

    public IController Controller1
    {
        get => Joypad.Controller1;
        set => Joypad.Controller1 = value;
    }

    public IController Controller2
    {
        get => Joypad.Controller2;
        set => Joypad.Controller2 = value;
    }

    public void Dispose()
    {
        // TODO dispose accordingly
    }

    public void RunFrame()
    {
        Controller1.Update();
        Controller2.Update();

        // a lame main loop with a workaround to be able to underclock

        for (var i = 0; i < SYNC_LOOPS; i++)
        {
            for (var j = 0; j < SYNC_CYCLES; j++)
            {
                Cpu.Run(); // Cpu.handleInterrupts();
            }

            Bus.tick(SYNC_CYCLES * MIPS_UNDERCLOCK);

            Cpu.handleInterrupts();
        }
    }

    public void ToggleDebug()
    {
        Cpu.debug     = !Cpu.debug;
        Gpu.Debugging = !Gpu.Debugging;
    }

    public void ToggleCdromLid()
    {
        Cdrom.toggleLid();
    }
}