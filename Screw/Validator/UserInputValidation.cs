using System;
using System.Windows.Forms;

namespace Screw.Validator
{
    /// <summary>
    /// Class for the "CheckNumberKeyPressed" event handler.
    /// </summary>
    public class UserInputValidation
    {
        /// <summary>
        /// Validates only numbers for input.
        /// </summary>
        /// If is missing in [0-9] or a delimiter is entered -- set event handled.
        /// Separator - dot or comma -> entered number must contain only 1 dot or only 1 comma.
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CheckNumberKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsControl(e.KeyChar))
                && !(Char.IsDigit(e.KeyChar))
                && !((e.KeyChar == '.') && (((TextBox)sender).Text.IndexOf(".") == -1))
                )
            {
                e.Handled = true;
            }
        }
    }
}
