using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Net;
using UWPStreamer;
using Windows.Storage;

namespace InputRedirectionNTR
{
    public class NTRInputRedirection
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        string ipAddress;
        byte[] data = new byte[12];
        uint oldbuttons = 0xFFF;
        uint newbuttons = 0xFFF;
        uint oldtouch = 0x2000000;
        uint newtouch = 0x2000000;
        uint oldcpad = 0x800800;
        uint newcpad = 0x800800;
        uint touchclick = 0x00;
        uint cpadclick = 0x00;
        int Mode = 0;
        Keys[] ipKeysToCheck = { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9, Keys.Decimal, Keys.OemPeriod, Keys.Back, Keys.Delete, Keys.Escape };
        Keys[] buttonKeysToCheck = { Keys.A, Keys.B, Keys.RightShift, Keys.LeftShift, Keys.Enter, Keys.Right, Keys.Left, Keys.Up, Keys.Down, Keys.R, Keys.L, Keys.X, Keys.Y, Keys.Escape };
        Keys[] KeyboardInput = { Keys.A, Keys.S, Keys.N, Keys.M, Keys.H, Keys.F, Keys.T, Keys.G, Keys.W, Keys.Q, Keys.Z, Keys.X, Keys.Right, Keys.Left, Keys.Up, Keys.Down };
        uint[] GamePadInput = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x020, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800 };
        string[] ButtonNames = { "A", "B", "Select", "Start", "DPad Right", "DPad Left", "DPad Up", "DPad Down", "R", "L", "X", "Y" };
        Keys UpKey;
        bool WaitForKeyUp;
        bool debug = false;
        KeyboardState keyboardState;
        GamePadState gamePadState;
        uint KeyIndex;
        uint OldButton;
        public bool useGamePad = true;

        public NTRInputRedirection()
        {
            ipAddress = localSettings.Values["ip"].ToString();
        }

        public void CheckConnection()
        {
            if (!App.Connected)
            {
                App.scriptHelper.connect(ipAddress, 8000);
            }
        }

        public void ReadMain()
        {
            if (!WaitForKeyUp)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.F1))
                {
                    WaitForKeyUp = true;
                    UpKey = Keys.F1;
                    Mode = 1;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.F2))
                {
                    WaitForKeyUp = true;
                    UpKey = Keys.F2;
                    Mode = 2;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.F3))
                {
                    WaitForKeyUp = true;
                    UpKey = Keys.F3;
                    Mode = 3;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.F4))
                {
                    WaitForKeyUp = true;
                    UpKey = Keys.F4;
                    debug = !debug;

                }

                if (Keyboard.GetState().IsKeyDown(Keys.F5))
                {
                    WaitForKeyUp = true;
                    UpKey = Keys.F5;
                    useGamePad = !useGamePad;

                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyUp(UpKey))
                {
                    WaitForKeyUp = false;
                }
            }

            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            newbuttons = 0x00;
            //Keyboard
            for (int i = 0; i < GamePadInput.Length; i++)
            {
                if (keyboardState.IsKeyDown(KeyboardInput[i]))
                {
                    newbuttons += (uint)(0x01 << i);
                }
            }

            //GamePad
            if (useGamePad)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[0]) != GamePadInput[0])
                    {
                        newbuttons += GamePadInput[0];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[1]) != GamePadInput[1])
                    {
                        newbuttons += GamePadInput[1];
                    }
                }


                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[2]) != GamePadInput[2])
                    {
                        newbuttons += GamePadInput[2];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[3]) != GamePadInput[3])
                    {
                        newbuttons += GamePadInput[3];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[4]) != GamePadInput[4])
                    {
                        newbuttons += GamePadInput[4];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[5]) != GamePadInput[5])
                    {
                        newbuttons += GamePadInput[5];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[6]) != GamePadInput[6])
                    {
                        newbuttons += GamePadInput[6];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[7]) != GamePadInput[7])
                    {
                        newbuttons += GamePadInput[7];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.RightShoulder == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[8]) != GamePadInput[8])
                    {
                        newbuttons += GamePadInput[8];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.LeftShoulder == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[9]) != GamePadInput[9])
                    {
                        newbuttons += GamePadInput[9];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[10]) != GamePadInput[10])
                    {
                        newbuttons += GamePadInput[10];
                    }
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed)
                {
                    if ((newbuttons & GamePadInput[11]) != GamePadInput[11])
                    {
                        newbuttons += GamePadInput[11];
                    }
                }
            }

            newbuttons ^= 0xFFF;

                touchclick = 0x00;
                if (useGamePad)
                {
                    if (GamePad.GetState(PlayerIndex.One).Buttons.RightStick == ButtonState.Pressed)
                    {
                        newtouch = (uint)Math.Round(2047.5 + (GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X * 2047.5));
                        newtouch += (uint)Math.Round(2047.5 + (-GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y * 2047.5)) << 0x0C;
                        newtouch += 0x1000000;
                    }
                    else
                    {
                        newtouch = 0x2000000;
                    }
                }
                else
                {
                    newtouch = 0x2000000;
                }
            

                cpadclick = 0x00;
                newcpad = (uint)Math.Round(2047.5 + (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X * 2047.5));
                newcpad += (uint)Math.Round(4095 - (2047.5 + (-GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y * 2047.5))) << 0x0C;

                if (newcpad == 0x800800)
                {

                    if (Keyboard.GetState().IsKeyDown(KeyboardInput[12]))
                    {
                        newcpad = 0xFFF + (((newcpad >> 0x0C) & 0xFFF) << 0x0C);
                    }

                    if (Keyboard.GetState().IsKeyDown(KeyboardInput[13]))
                    {
                        newcpad = (((newcpad >> 0x0C) & 0xFFF) << 0x0C);
                    }

                    if (Keyboard.GetState().IsKeyDown(KeyboardInput[15]))
                    {
                        newcpad = (newcpad & 0xFFF) + (0x00 << 0x0C);
                    }

                    if (Keyboard.GetState().IsKeyDown(KeyboardInput[14]))
                    {
                        newcpad = (newcpad & 0xFFF) + (0xFFF << 0x0C);
                    }
                }

                if (newcpad != 0x800800)
                {
                    newcpad += 0x1000000;
                }

            SendInput();
        }

        private string GetButtonNameFromValue(uint value)
        {
            string result = "None";

            for (int i = 0; i < ButtonNames.Length; i++)
            {
                if ((value >> i) == 0x01)
                {
                    result = ButtonNames[i];
                    break;
                }
            }

            return result;
        }

        private void SendInput()
        {
            if ((newbuttons != oldbuttons) || (newtouch != oldtouch) || (newcpad != oldcpad))
            {
                oldbuttons = newbuttons;
                oldtouch = newtouch;
                oldcpad = newcpad;

                //Buttons
                data[0x00] = (byte)(oldbuttons & 0xFF);
                data[0x01] = (byte)((oldbuttons >> 0x08) & 0xFF);
                data[0x02] = (byte)((oldbuttons >> 0x10) & 0xFF);
                data[0x03] = (byte)((oldbuttons >> 0x18) & 0xFF);

                //Touch
                data[0x04] = (byte)(oldtouch & 0xFF);
                data[0x05] = (byte)((oldtouch >> 0x08) & 0xFF);
                data[0x06] = (byte)((oldtouch >> 0x10) & 0xFF);
                data[0x07] = (byte)((oldtouch >> 0x18) & 0xFF);

                //CPad
                data[0x08] = (byte)(oldcpad & 0xFF);
                data[0x09] = (byte)((oldcpad >> 0x08) & 0xFF);
                data[0x0A] = (byte)((oldcpad >> 0x10) & 0xFF);
                data[0x0B] = (byte)((oldcpad >> 0x18) & 0xFF);

                CheckConnection();
                if (App.Connected)
                {
                    App.scriptHelper.write(0x10DF20, data, 0x10);
                }
            }
        }
    }
}
