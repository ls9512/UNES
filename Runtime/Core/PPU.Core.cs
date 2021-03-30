using System;

namespace Aya.UNES
{
    partial class PPU
    {
        private const int GameWidth = 256, GameHeight = 240;

        private uint _bufferPos;
        public readonly uint[] RawBitmap = new uint[GameWidth * GameHeight];
        private readonly uint[] _priority = new uint[GameWidth * GameHeight];

        // TODO: use real chroma/luma decoding
        private readonly uint[] _palette = {
            0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400,
            0x503000, 0x007800, 0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000,
            0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC, 0xD800CC, 0xE40058, 0xF83800, 0xE45C10,
            0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000, 0x000000, 0x000000,
            0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
            0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000,
            0xFCFCFC, 0xA4E4FC, 0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8,
            0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8, 0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
        };

        private int _scanLineCount = 261;
        private int _cyclesPerLine = 341;
        private int _cpuSyncCounter;
        private readonly uint[] _scanLineOAM = new uint[8 * 4];
        private readonly bool[] _isSprite0 = new bool[8];
        private int _spriteCount;

        private long _tileShiftRegister;
        private uint _currentNameTableByte;
        private uint _currentHighTile, _currentLowTile;
        private uint _currentColor;

        public void ProcessPixel(int x, int y)
        {
            ProcessBackgroundForPixel(x, y);
            if (F.DrawSprites)
            {
                ProcessSpritesForPixel(x, y);
            }

            if (y != -1)
            {
                _bufferPos++;
            }
        }

        private void CountSpritesOnLine(int scanLine)
        {
            _spriteCount = 0;
            var height = F.TallSpritesEnabled ? 16 : 8;

            for (var idx = 0; idx < _oam.Length; idx += 4)
            {
                var y = _oam[idx] + 1;
                if (scanLine >= y && scanLine < y + height)
                {
                    _isSprite0[_spriteCount] = idx == 0;
                    _scanLineOAM[_spriteCount * 4 + 0] = _oam[idx + 0];
                    _scanLineOAM[_spriteCount * 4 + 1] = _oam[idx + 1];
                    _scanLineOAM[_spriteCount * 4 + 2] = _oam[idx + 2];
                    _scanLineOAM[_spriteCount * 4 + 3] = _oam[idx + 3];
                    _spriteCount++;
                }

                if (_spriteCount == 8)
                {
                    break;
                }
            }
        }

        private void NextNameTableByte()
        {
            _currentNameTableByte = ReadByte(0x2000 | (V & 0x0FFF));
        }

        private void NextTileByte(bool hi)
        {
            var tileIdx = _currentNameTableByte * 16;
            var address = F.PatternTableAddress + tileIdx + FineY;

            if (hi)
            {
                _currentHighTile = ReadByte(address + 8);
            }
            else
            {
                _currentLowTile = ReadByte(address);
            }
        }

        private void NextAttributeByte()
        {
            // Bless nesdev
            var address = 0x23C0 | (V & 0x0C00) | ((V >> 4) & 0x38) | ((V >> 2) & 0x07);
            _currentColor = (ReadByte(address) >> (int)((CoarseX & 2) | ((CoarseY & 2) << 1))) & 0x3;
        }

        private void ShiftTileRegister()
        {
            for (var x = 0; x < 8; x++)
            {
                uint palette = ((_currentHighTile & 0x80) >> 6) | ((_currentLowTile & 0x80) >> 7);
                _tileShiftRegister |= (palette + _currentColor * 4) << ((7 - x) * 4);
                _currentLowTile <<= 1;
                _currentHighTile <<= 1;
            }
        }

        private void ProcessBackgroundForPixel(int cycle, int scanLine)
        {
            if (cycle < 8 && !F.DrawLeftBackground || !F.DrawBackground && scanLine != -1)
            {
                // Maximally sketchy: if current address is in the PPU palette, then it draws that palette entry if rendering is disabled
                // Otherwise, it draws $3F00 (universal bg color)
                // https://www.romhacking.net/forum/index.php?topic=20554.0
                // Don't know if any game actually uses it, but a test ROM I wrote unexpectedly showed this
                // corner case
                RawBitmap[_bufferPos] = _palette[ReadByte(0x3F00 + ((F.BusAddress & 0x3F00) == 0x3F00 ? F.BusAddress & 0x001F : 0)) & 0x3F];
                return;
            }

            var paletteEntry = (uint)(_tileShiftRegister >> 32 >> (int)((7 - X) * 4)) & 0x0F;
            if (paletteEntry % 4 == 0) paletteEntry = 0;

            if (scanLine != -1)
            {
                _priority[_bufferPos] = paletteEntry;
                RawBitmap[_bufferPos] = _palette[ReadByte(0x3F00u + paletteEntry) & 0x3F];
            }
        }

        private void ProcessSpritesForPixel(int x, int scanLine)
        {
            for (var idx = _spriteCount * 4 - 4; idx >= 0; idx -= 4)
            {
                var spriteX = _scanLineOAM[idx + 3];
                var spriteY = _scanLineOAM[idx] + 1;

                // Don't draw this sprite if...
                if (spriteY == 0 || // it's located at y = 0
                    spriteY > 239 || // it's located past y = 239 ($EF)
                    x >= spriteX + 8 || // it's behind the current dot
                    x < spriteX || // it's ahead of the current dot
                    x < 8 && !F.DrawLeftSprites) // it's in the clip area, and clipping is enabled
                {
                    continue;
                }

                // amusingly enough, the PPU's palette handling is basically identical
                // to that of the Gameboy / Gameboy Color, so I've sort of just copy/pasted
                // handling code wholesale from my GBC emulator at
                // https://github.com/Xyene/Nitrous-Emulator/blob/master/src/main/java/nitrous/lcd/LCD.java#L642
                var tileIdx = _scanLineOAM[idx + 1];
                if (F.TallSpritesEnabled)
                {
                    tileIdx &= ~0x1u;
                }

                tileIdx *= 16;

                var attribute = _scanLineOAM[idx + 2] & 0xE3;

                var palette = attribute & 0x3;
                var front = (attribute & 0x20) == 0;
                var flipX = (attribute & 0x40) > 0;
                var flipY = (attribute & 0x80) > 0;

                var px = (int) (x - spriteX);
                var line = (int) (scanLine - spriteY);

                var tableBase = F.TallSpritesEnabled ? (_scanLineOAM[idx + 1] & 1) * 0x1000 : F.SpriteTableAddress;

                if (F.TallSpritesEnabled)
                {
                    if (line >= 8)
                    {
                        line -= 8;
                        if (!flipY)
                        {
                            tileIdx += 16;
                        }

                        flipY = false;
                    }

                    if (flipY)
                    {
                        tileIdx += 16;
                    }
                }

                // here we handle the x and y flipping by tweaking the indices we are accessing
                var logicalX = flipX ? 7 - px : px;
                var logicalLine = flipY ? 7 - line : line;

                var address = (uint) (tableBase + tileIdx + logicalLine);

                // this looks bad, but it's about as readable as it's going to get
                var color = (uint) (
                    (
                        (
                            (
                                // fetch upper bit from 2nd bit plane
                                ReadByte(address + 8) & (0x80 >> logicalX)
                            ) >> (7 - logicalX)
                        ) << 1 // this is the upper bit of the color number
                    ) |
                    (
                        (
                            ReadByte(address) & (0x80 >> logicalX)
                        ) >> (7 - logicalX)
                    )); // << 0, this is the lower bit of the color number

                if (color > 0)
                {
                    var backgroundPixel = _priority[_bufferPos];
                    // Sprite 0 hits...
                    if (!(!_isSprite0[idx / 4] || // do not occur on not-0 sprite
                          x < 8 && !F.DrawLeftSprites || // or if left clipping is enabled
                          backgroundPixel == 0 || // or if bg pixel is transparent
                          F.Sprite0Hit || // or if it fired this frame already
                          x == 255)) // or if x is 255, "for an obscure reason related to the pixel pipeline"
                    {
                        F.Sprite0Hit = true;
                    }

                    if (F.DrawBackground && (front || backgroundPixel == 0))
                    {
                        if (scanLine != -1)
                        {
                            RawBitmap[_bufferPos] = _palette[ReadByte(0x3F10 + palette * 4 + color) & 0x3F];
                        }
                    }
                }
            }
        }

        public void ProcessFrame()
        {
            RawBitmap.Fill(0u);
            _priority.Fill(0u);
            _bufferPos = 0;

            for (var i = -1; i < _scanLineCount; i++)
            {
                ProcessScanLine(i);
            }
        }

        public void ProcessScanLine(int line)
        {
            for (var i = 0; i < _cyclesPerLine; i++)
            {
                ProcessCycle(line, i);
            }
        }

        private int _cpuClocksSinceVBL;
        private int _ppuClocksSinceVBL;

        public void ProcessCycle(int scanLine, int cycle)
        {
            var visibleCycle = 1 <= cycle && cycle <= 256;
            var prefetchCycle = 321 <= cycle && cycle <= 336;
            var fetchCycle = visibleCycle || prefetchCycle;

            if (F.VBlankStarted)
            {
                _ppuClocksSinceVBL++;
            }

            if (0 <= scanLine && scanLine < 240 || scanLine == -1)
            {
                if (visibleCycle)
                {
                    ProcessPixel(cycle - 1, scanLine);
                }

                // During pixels 280 through 304 of this scanline, the vertical scroll bits are reloaded TODO: if rendering is enabled.
                if (scanLine == -1 && 280 <= cycle && cycle <= 304)
                {
                    ReloadScrollY();
                }

                if (fetchCycle)
                {
                    _tileShiftRegister <<= 4;

                    // See https://wiki.nesdev.com/w/images/d/d1/Ntsc_timing.png
                    // Takes 8 cycles for tile to be read, 2 per "step"
                    switch (cycle & 7)
                    {
                        case 1: // NT
                            NextNameTableByte();
                            break;
                        case 3: // AT
                            NextAttributeByte();
                            break;
                        case 5: // Tile low
                            NextTileByte(false);
                            break;
                        case 7: // Tile high
                            NextTileByte(true);
                            break;
                        case 0: // 2nd cycle of tile high fetch
                            if (cycle == 256)
                                IncrementScrollY();
                            else
                                IncrementScrollX();
                            // Begin rendering a brand new tile
                            ShiftTileRegister();
                            break;
                    }
                }

                if (cycle == 257)
                {
                    ReloadScrollX();
                    // 257 - 320
                    // The tile data for the sprites on the next scanline are fetched here.
                    // TODO: stagger this over all the cycles as opposed to only on 257
                    CountSpritesOnLine(scanLine + 1);
                }
            }

            // TODO: this is a hack; VBlank should be cleared on dot 1 of the pre-render line,
            // but for some reason we're at 2272-2273 CPU clocks at that time
            // (i.e., our PPU timing is off somewhere by 6-9 PPU cycles per frame)
            if (F.VBlankStarted && _cpuClocksSinceVBL == 2270)
            {
                F.VBlankStarted = false;
                _cpuClocksSinceVBL = 0;
            }

            if (cycle == 1)
            {
                if (scanLine == 241)
                {
                    F.VBlankStarted = true;
                    if (F.NMIEnabled)
                    {
                        _emulator.CPU.TriggerInterrupt(CPU.InterruptType.NMI);
                    }
                }

                // Happens at the same time as 1st cycle of NT byte fetch
                if (scanLine == -1)
                {
                    // Console.WriteLine(_ppuClocksSinceVBL);
                    _ppuClocksSinceVBL = 0;
                    F.VBlankStarted = false;
                    F.Sprite0Hit = false;
                    F.SpriteOverflow = false;
                }
            }

            _emulator.Mapper.ProcessCycle(scanLine, cycle);

            if (_cpuSyncCounter + 1 == 3)
            {
                if (F.VBlankStarted)
                {
                    _cpuClocksSinceVBL++;
                }

                _emulator.CPU.TickFromPPU();
                _cpuSyncCounter = 0;
            }
            else
            {
                _cpuSyncCounter++;
            }
        }
    }
}
