using System;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using Screw.Error;
using Screw.Manager;
using Screw.Validator;
using Screw.Model;


namespace Screw
{

    /// <summary>
    /// Main form of program
    /// </summary>
    public partial class ScrewView : Form
    {
        /// <summary>
        /// Kompas object manager
        /// </summary>
        private KompasApplication _kompasApp;

        /// <summary>
        /// Figure parameters
        /// </summary>
        private List<double> _figureParameters;

        /// <summary>
        /// Screw view form constructor
        /// </summary>
        public ScrewView()
        {
            InitializeComponent();

            CloseKompas3D.Enabled = false;
            RunButton.Enabled = false;
            Defaults.Enabled = false;

            WithoutHoleRadioButton.Enabled = false;
            CrossheadScrewdriverRadioButton.Enabled = false;
            FlatheadScrewdriverRadioButton.Enabled = false;
            RegularPolygonScrewdriverRadioButton.Enabled = false;

            SetAllInputsEnabledState(false);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Set state to all inputs "Enabled" property
        /// </summary>
        /// <param name="state">State of "Enabled" property</param>
        private void SetAllInputsEnabledState(bool state)
        {
            foreach (Control control in Controls)
            {
                if (control.GetType() == typeof(GroupBox))
                {
                    foreach (Control combobox in control.Controls)
                    {
                        if (combobox.GetType() == typeof(ComboBox))
                        {
                            combobox.Enabled = state;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set figure parameters
        /// </summary>
        /// <returns>true if the operation was successful; false в error case</returns>
        private bool SetFigureParameters()
        {
            try
            {
                var screwHatWidth = Convert.ToDouble(ScrewHatWidth.Text);
                var screwHatInnerDiameter = Convert.ToDouble(this.screwHatInnerDiameter.Text);
                var screwBaseSmoothWidth = Convert.ToDouble(ScrewBaseSmoothWidth.Text);
                var screwBaseThreadWidth = Convert.ToDouble(ScrewBaseThreadWidth.Text);
                var nutHeight = Convert.ToDouble(NutHeight.Text);
                var nutThreadDiameter = Convert.ToDouble(NutThreadDiameter.Text);

                var parameters = new List<double>() {screwHatWidth,
                    screwHatInnerDiameter,
                    screwBaseSmoothWidth,
                screwBaseThreadWidth, nutHeight, nutThreadDiameter};

                var validator = new FigureParametersValidator(parameters);
                if (validator.LastErrorCode != ErrorCodes.OK)
                {
                    return false;
                }

                if (!validator.Validate())
                {
                    var errorCatcher = new UserInputErrorCatcher();
                    errorCatcher.CatchError(validator.ErrorList);

                    return false;
                }

                _figureParameters = parameters;
            }
            catch
            {
                MessageBox.Show("There are some empty or invalid fields. " +
                    "Please fill them in correctly and try again. ", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Sends a control to a class to validate user input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckNumberKeyPressed(object sender, KeyPressEventArgs e)
        {
            UserInputValidation.CheckNumberKeyPressed(sender, e);
        }

        private void LoadKompas3D_Click(object sender, EventArgs e)
        {
            if (LoadKompas3D.Enabled)
            {
                var errorCatcher = new ErrorCatcher();

                _kompasApp = new KompasApplication();
                if (_kompasApp == null)
                {
                    errorCatcher.CatchError(
                        ErrorCodes.KompasObjectCreatingError);
                }
                if (_kompasApp.LastErrorCode != ErrorCodes.OK)
                {
                    errorCatcher.CatchError(_kompasApp.LastErrorCode);
                    return;
                }

                SetAllInputsEnabledState(true);

                RunButton.Enabled = true;
                Defaults.Enabled = true;

                WithoutHoleRadioButton.Enabled = true;
                CrossheadScrewdriverRadioButton.Enabled = true;
                FlatheadScrewdriverRadioButton.Enabled = true;
                RegularPolygonScrewdriverRadioButton.Enabled = true;

                LoadKompas3D.Enabled = false;
                CloseKompas3D.Enabled = true;
            }
        }

        /// <summary>
		/// Get selected screwdriver type.
		/// </summary>
		/// <returns>Selected screwdriver type.</returns>
		private Screwdriver GetSelectedScrewdriverType()
        {
            if (WithoutHoleRadioButton.Checked)
            {
                return Screwdriver.WithoutHole;
            }
            else if (CrossheadScrewdriverRadioButton.Checked)
            {
                return Screwdriver.CrossheadScrewdriver;
            }
            else if (FlatheadScrewdriverRadioButton.Checked)
            {
                return Screwdriver.FlatheadScrewdriver;
            }
            else if (RegularPolygonScrewdriverRadioButton.Checked)
            {
                return Screwdriver.RegularPolygonScrewdriver;
            }

            throw new ArgumentException(
                "Screwdriver is not checked in list of screwdriver types.");
        }

        /// <summary>
        /// Validating user input options and building the figure after that
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunButton_Click_1(object sender, EventArgs e)
        {
            var errorCatcher = new ErrorCatcher();

            if (_kompasApp == null)
            {
                MessageBox.Show("Download first KOMPAS 3D.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            if (!SetFigureParameters())
            {
                return;
            }

            _kompasApp.Parameters = _figureParameters;

            if (!_kompasApp.CreateDocument3D())
            {
                return;
            }

            BuildManager _buildManager = new BuildManager(_kompasApp);
            if (_buildManager == null)
            {
                errorCatcher.CatchError(ErrorCodes.ManagerCreatingError);
                return;
            }
            if (_buildManager.LastErrorCode != ErrorCodes.OK)
            {
                errorCatcher.CatchError(_buildManager.LastErrorCode);
                return;
            }

            _buildManager.ScrewdriverType = GetSelectedScrewdriverType();
            _buildManager.CreateDetail();

            if (_buildManager.LastErrorCode != ErrorCodes.OK)
            {
                errorCatcher.CatchError(_buildManager.LastErrorCode);
            }
            else
            {
                errorCatcher.CatchSuccess();
            }
        }

        /// <summary>
        /// Unset Kompas 3D object from controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseKompas3D_Click(object sender, EventArgs e)
        {
            if (!LoadKompas3D.Enabled)
            {
                _kompasApp.DestructApp();

                SetAllInputsEnabledState(false);

                RunButton.Enabled = false;

                LoadKompas3D.Enabled = true;
                CloseKompas3D.Enabled = false;
            }
        }

        /// <summary>
        /// Setting default parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Defaults_Click(object sender, EventArgs e)
        {
            Default parametrs = new Default();

            ScrewHatWidth.Text = parametrs._diameter.ToString();
            screwHatInnerDiameter.Text = parametrs._slotDepth.ToString();
            ScrewBaseSmoothWidth.Text = parametrs._smoothPart.ToString();
            ScrewBaseThreadWidth.Text = parametrs._threadPart.ToString();
            NutHeight.Text = parametrs._hatHeight.ToString();
            NutThreadDiameter.Text = parametrs._slotWidth.ToString();
        }

        private void ScrewHatWidth_ChangeText(object sender, EventArgs e)
        {
            var tmp = ScrewHatWidth.Text;
            var getValue = 0.0;
            var maxLength = 0.0;
            try
            {
                getValue = Convert.ToDouble(tmp);
                maxLength = getValue / 5;
            }
            catch {
                if (tmp == "")
                {
                    getValue = 0;
                }
            }
            if (getValue >= 10 && getValue <= 45)
            {
                NutThreadDiameterLabel.Text = $"Slot width (n) (2.5-{maxLength})mm";
            }
        }

        private void FlatheadScrewdriverRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (FlatheadScrewdriverRadioButton.Checked)
            {
                ScrewHatWidthLabel.Text = "Hat diameter (D) (27-45)mm";
                ScrewBaseSmoothPartLabel.Text = "Smooth part (l) (5-35)mm";
                ScrewBaseThreadPartLabel.Text = "Thread part (b) (5-80)mm";
                NutHeightLabel.Text = "Hat height (H) (6-20)mm";
                screwHatInnerDiameter.Text = 4.ToString();
                NutThreadDiameter.Text = 2.5.ToString();
                screwHatInnerDiameter.Visible = false;
                NutThreadDiameter.Visible = false;
                ScrewHatInnerCircleLabel.Visible = false;
                NutThreadDiameterLabel.Visible = false;
            }
        }

        private void RegularPolygonScrewdriverRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (RegularPolygonScrewdriverRadioButton.Checked)
            {
                ScrewHatInnerCircleLabel.Visible = true;
                NutThreadDiameterLabel.Visible = true;
                screwHatInnerDiameter.Visible = true;
                NutThreadDiameter.Visible = true;
                screwHatInnerDiameter.Text = "";
                NutThreadDiameter.Text = "";
                ScrewHatWidthLabel.Text = "Hat diameter (D) (27-45)mm";
                ScrewBaseSmoothPartLabel.Text = "Smooth part (l) (5-35)mm";
                ScrewBaseThreadPartLabel.Text = "Thread part (b) (5-80)mm";
                NutHeightLabel.Text = "Hat height (H) (6-20)mm";
                ScrewHatInnerCircleLabel.Text = "Slot depth (m) (4-8)mm";
                NutThreadDiameterLabel.Text = "Slot width (n) (2.5-9)mm";
            }
        }

        private void ScrewView_Load(object sender, EventArgs e)
        {

        }
    }
}
