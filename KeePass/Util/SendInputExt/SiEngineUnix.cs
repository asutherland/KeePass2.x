﻿/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2014 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;

using KeePass.Native;

namespace KeePass.Util.SendInputExt
{
	internal sealed class SiEngineUnix : SiEngineStd
	{
		private bool m_bFirst = true;

		public override void Init()
		{
			base.Init();

			m_bFirst = true;
		}

		// public override void Release()
		// {
		//	base.Release();
		// }

		public override void SendKeyImpl(int iVKey, bool? bExtKey, bool? bDown)
		{
			SiCode si = SiCodes.Get(iVKey, bExtKey);
			if(si == null)
			{
				char ch = SiCodes.VKeyToChar(iVKey);
				if(ch != char.MinValue) SendCharImpl(ch, bDown);
				return;
			}

			string strXKeySym = si.XKeySym;
			if(string.IsNullOrEmpty(strXKeySym)) { Debug.Assert(false); return; }

			string strVerb = "key";
			if(bDown.HasValue) strVerb = (bDown.Value ? "keydown" : "keyup");

			RunXDoTool(strVerb, strXKeySym);
		}

		public override void SetKeyModifierImpl(Keys kMod, bool bDown)
		{
			string strVerb = (bDown ? "keydown" : "keyup");

			if((kMod & Keys.Shift) != Keys.None)
				RunXDoTool(strVerb, "shift");
			if((kMod & Keys.Control) != Keys.None)
				RunXDoTool(strVerb, "ctrl");
			if((kMod & Keys.Alt) != Keys.None)
				RunXDoTool(strVerb, "alt");
		}

		public override void SendCharImpl(char ch, bool? bDown)
		{
			string strVerb = "key";
			if(bDown.HasValue) strVerb = (bDown.Value ? "keydown" : "keyup");

			// Unicode is supported; codes are 'UHHHH' with 'HHHH' being
			// the Unicode value; see header of 'keysymdef.h'
			RunXDoTool(strVerb, "U" + ((int)ch).ToString("X4",
				NumberFormatInfo.InvariantInfo));
		}

		private void RunXDoTool(string strVerb, string strParam)
		{
			if(string.IsNullOrEmpty(strVerb)) { Debug.Assert(false); return; }

			string str = strVerb;

			if(m_bFirst)
			{
				str += " --clearmodifiers";
				m_bFirst = false;
			}

			if(!string.IsNullOrEmpty(strParam))
				str += " " + strParam;

			NativeMethods.RunXDoTool(str);
		}
	}
}