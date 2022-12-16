using System.Collections.Generic;
using System.Globalization;
using KompasAPI7;
using Screw.Error;
using System;

namespace Screw.Validator
{
    /// <summary>
    /// Validator of parameters of figures of build
    /// </summary>
    public class FigureParametersValidator : IValidator
    {

        /// <summary>
        /// Figure parameters
        /// </summary>
        private List<double> _figureParameters;

        /// <summary>
        /// List with errors
        /// </summary>
        public List<string> ErrorList
        {
            get;
            private set;
        }

        /// <summary>
        /// Last error code
        /// </summary>
        public ErrorCodes LastErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Figure parameters constructor
        /// </summary>
        /// <param name="parameters">List of figure parameters</param>
        public FigureParametersValidator(List<double> parameters)
        {
            ErrorList = new List<string>() { };

            if (parameters.Count != 6)
            {
                LastErrorCode = ErrorCodes.ArgumentInvalid;
                return;
            }
            _figureParameters = parameters;
        }

        /// <summary>
        /// Validate all chain by set of rules
        /// </summary>
        /// <returns>true if validation successful</returns>
        public bool Validate()
        {
            var screwHatHeight = _figureParameters[4];
            var screwBaseWidth = _figureParameters[2] + _figureParameters[3];

            var diapasonStart = default(double);
            var diapasonEnd = default(double);
            var errorMessage = default(string);

            if (!ValidateDoubles()) return false;

            if (!(_figureParameters[1] < screwHatHeight))
            {
                diapasonStart = screwHatHeight;
                errorMessage = string.Format(CultureInfo.InvariantCulture, 
                    "Slot depth (m) can't be more or equals Hat height (H)",
                    diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if ((screwBaseWidth <= screwHatHeight))
            {
                diapasonStart = screwBaseWidth;
                diapasonEnd = screwHatHeight;
                errorMessage = string.Format(CultureInfo.InvariantCulture, 
                    "Hat height (H) can't be more or equals Smooth part (l) + Thread part (b)",
                    diapasonStart);
                errorMessage += "\n(This parameter depends of l and b)";
                ErrorList.Add(errorMessage);
            }

            if (!(_figureParameters[2] <= _figureParameters[3]))
            {
                diapasonStart = _figureParameters[2];
                diapasonEnd = _figureParameters[3];
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Smooth part (l) can't be more than Thread part (b)",
                    diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[0] < 15)
            {
                diapasonStart = 15;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Hat diameter (D) must be more or equals 15", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[0] > 45)
            {
                diapasonStart = 45;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Hat diameter (D) must be less or equals 45", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[1] < 4)
            {
                diapasonStart = 4;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Slot depth (m) must be more or equals 4", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[1] > 8)
            {
                diapasonStart = 8;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Slot depth (m) must be less or equals 8", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[2] < 5)
            {
                diapasonStart = 5;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Smooth part (l) must be more or equals 5", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[2] > 35)
            {
                diapasonStart = 35;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Smooth part (l) must be less or equals 35", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[3] < 5)
            {
                diapasonStart = 5;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Thread part (b) must be more or equals 5", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[3] > 80)
            {
                diapasonStart = 80;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Thread part (b) must be less or equals 80", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[4] < 6)
            {
                diapasonStart = 6;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Hat height (H) must be more or equals 6", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[4] > 20)
            {
                diapasonStart = 20;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Hat height (H) must be less or equals 20", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[5] < 2)
            {
                diapasonStart = 2;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Slot width (n) must be more or equals 2", diapasonStart);
                ErrorList.Add(errorMessage);
            }

            if (_figureParameters[5] > (_figureParameters[0] / 5))
            {
                diapasonStart = _figureParameters[0] / 5;
                errorMessage = string.Format(CultureInfo.InvariantCulture,
                    "Slot width (n) must be less "+diapasonStart, diapasonStart);
                ErrorList.Add(errorMessage);
            }

            return ErrorList.Count == 0;
        }

        /// <summary>
        /// Checking for double values ​​in shape parameters.
        /// </summary>
        /// <returns> true, if the check was successful</returns>
        private bool ValidateDoubles()
        {
            foreach (double parameter in _figureParameters)
            {
                if (parameter <= 0)
                {
                    ErrorList.Add("Parameter can't be 0");
                    return false;
                }
                if (parameter < 0.1)
                {
                    ErrorList.Add("The parameter must be greater than 0.1");
                    return false;
                }
                if (parameter >= 1000)
                {
                    ErrorList.Add("Parameter must be less than 1000");
                }
                if (!DoubleValidator.Validate(parameter))
                {
                    ErrorList.Add("Incorrect parameter value");
                    return false;
                }
            }

            return true;
        }

    }
}
