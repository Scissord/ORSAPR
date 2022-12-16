using Kompas6API5;
using Kompas6Constants3D;
using Screw.Model.Point;
using Screw.Error;
using Screw.Validator;

namespace Screw.Model.FigureParam
{
    /// <summary>
    /// Spin class.
    /// Presents spin parameters of 2D document.
    /// </summary>
   public class Spin
   {
        /// <summary>
        /// Last error code
        /// </summary>
        public ErrorCodes LastErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Figure param
        /// </summary>
        public ksEntity Entity
        {
            get;
            private set;
        }

        /// <summary>
        /// Spin step
        /// </summary>
        public double SpinStep
        {
            get;
            private set;
        }

        /// <summary>
        /// Parameter of rotation on the faces of rotation(beginning and end),
        /// rotation location, diameter size and rotation step
        /// </summary>
        /// <param name="parameters">Parameters of spin</param>
        public Spin(SpinParameters parameters)
        {
            if (parameters.BeginSpinFace == null
                || parameters.EndSpinFace == null
                || parameters.SpinLocationPoint.LastErrorCode != ErrorCodes.OK
                || parameters.DiameterSize == default(double)
                || !DoubleValidator.Validate(parameters.DiameterSize)
                || parameters.SpinStep == default(double)
                || !DoubleValidator.Validate(parameters.SpinStep)
            )
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return;
            }

            if (parameters.DiameterSize <= default(double)
                || !DoubleValidator.Validate(parameters.DiameterSize)
                || !DoubleValidator.Validate(parameters.SpinLocationPoint.X)
                || !DoubleValidator.Validate(parameters.SpinLocationPoint.Y)
            )
            {
                LastErrorCode = ErrorCodes.DoubleValueValidationError;
                return;
            }

            if (!CreateSpin(parameters))
            {
                return;
            }
        }

        /// <summary>
        /// Creating a rotation definition based on origin
        /// and final essence, diameter and height of rotation
        /// </summary>
        /// <param name="parameters">Spin parameters</param>
        private bool CreateSpin(SpinParameters parameters)
        {
            var spin = (ksEntity)parameters.Document3DPart.NewEntity(
                (short)Obj3dType.o3d_cylindricSpiral);
            if (spin == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return false;
            }

            var spinDefinition = (ksCylindricSpiralDefinition)spin.GetDefinition();
            if (spinDefinition == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return false;
            }

            spinDefinition.SetPlane(parameters.BeginSpinFace);

            spinDefinition.buildDir = true;                 
            spinDefinition.buildMode = 1;                  
            spinDefinition.diamType = 0;                    
            spinDefinition.diam = parameters.DiameterSize;
            spinDefinition.heightType = 1;                 
            spinDefinition.SetHeightObject(parameters.EndSpinFace);
            spinDefinition.turnDir = true;                  
            spinDefinition.step = parameters.SpinStep;     

            if (!spinDefinition.SetLocation(parameters.SpinLocationPoint.X, 
                parameters.SpinLocationPoint.Y)
                || !spin.Create()
            )
            {
                LastErrorCode = ErrorCodes.EntityCreateError;
                return false;
            }

            Entity = spin;
            SpinStep = spinDefinition.step;

            return true;
        }
    }
}
