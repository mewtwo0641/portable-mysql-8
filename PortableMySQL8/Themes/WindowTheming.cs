#region License

/*

Copyright 2023 mewtwo0641
(See ADDITIONAL_COPYRIGHTS.txt for full list of copyright holders)

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

#endregion License

using ControlzEx.Theming;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortableMySQL8.Themes
{
    public class WindowTheming
    {
        public List<string> ValidAccents = new List<string>()
        {
            "Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald",
            "Green", "Indigo", "Lime", "Magenta", "Mauve", "Olive", "Orange",
            "Pink", "Purple", "Red", "Sienna", "Steel", "Taupe", "Teal",
            "Violet", "Yellow"
        };

        public List<string> ValidThemes = new List<string>()
        {
            "Light", "Dark"
        };

        public WindowTheming()
        {

        }

        public void ApplyTheme(string accent, string theme)
        {
            if (!ValidAccents.Contains(accent))
                throw new InvalidAccentException(String.Format("The accent '{0}' is not valid!", accent));

            if (!ValidThemes.Contains(theme))
                throw new InvalidThemeException(String.Format("The theme '{0}' is not valid!", theme));

            ThemeManager.Current.ChangeTheme(Application.Current, theme + "." + accent);
        }
    }

    #region Exceptions

    public class InvalidAccentException : Exception
    {
        public InvalidAccentException()
        {

        }

        public InvalidAccentException(string message) : base(message)
        {

        }
    }

    public class InvalidThemeException : Exception
    {
        public InvalidThemeException()
        {

        }

        public InvalidThemeException(string message) : base(message)
        {

        }
    }

    #endregion Exceptions
}
