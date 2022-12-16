using Kompas6API5;
using Kompas6Constants3D;
using Screw.Error;

namespace Screw.Model.Entity
{
    /// <summary>
    /// Sketch class.
    /// Represents sketch in 2D entity in detail.
    /// </summary>
    public class KompasSketch:ErrorObjCreation
    {
        /// <summary>
        /// Base plane fo sketch
        /// </summary>
        private ksEntity _basePlane;

        /// <summary>
        /// Axis of base plane
        /// </summary>
        private Obj3dType _basePlaneAxis;

        /// <summary>
        /// Sketch definition
        /// </summary>
        private ksSketchDefinition _sketchDef;

        /// <summary>
        /// Sketch entity getter
        /// </summary>
        public ksEntity Entity
        {
            get;
            private set;
        }

        /// <summary>
        /// Entity base plane setter
        /// </summary>
        public ksEntity BasePlane
        {
            get;
            private set;
        }

        /// <summary>
        /// Create sketch from reference plane
        /// </summary>
        /// <param name="doc3DPart">Document 3D part</param>
        /// <param name="basePlane">Base plane</param>
        public KompasSketch(ksPart doc3DPart, ksEntity basePlane)
        {
            if (doc3DPart == null || basePlane == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return;
            }

            _basePlane = basePlane;

            Entity = CreateEntity(doc3DPart);
        }

        /// <summary>
        /// Create sketch from reference plane
        /// </summary>
        /// <param name="doc3DPart">Document 3D part</param>
        /// <param name="basePlaneAxis">Base plane axis</param>
        public KompasSketch(ksPart doc3DPart, Obj3dType basePlaneAxis)
        {
            if (doc3DPart == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return;
            }

            if (!(basePlaneAxis == Obj3dType.o3d_planeXOY
                || basePlaneAxis == Obj3dType.o3d_planeXOZ
                || basePlaneAxis == Obj3dType.o3d_planeYOZ)
            )
            {
                LastErrorCode = ErrorCodes.ArgumentInvalid;
                return;
            }

            _basePlaneAxis = basePlaneAxis; 

            Entity = CreateEntity(doc3DPart);
        }

        /// <summary>
        /// Start editing object
        /// </summary>
        /// <returns>Kompas 2D document (editable sketch)</returns>
        public ksDocument2D BeginEntityEdit()
        {
            if (_sketchDef == null)
            {
                LastErrorCode = ErrorCodes.EntityDefinitionNull;
                return null;
            }
            return (ksDocument2D)_sketchDef.BeginEdit();
        }

        /// <summary>
        /// End of object editing
        /// </summary>
        public void EndEntityEdit()
        {
            _sketchDef.EndEdit();
        }

        /// <summary>
        /// Create object from reference plane
        /// </summary>
        /// <param name="doc3DPart">Part of 3D document (detail in build)</param>
        /// <returns>true if operation successful; false in case of error</returns>
        private ksEntity CreateEntity(ksPart doc3DPart)
        {
            var sketch = (ksEntity)doc3DPart.NewEntity((short)Obj3dType.o3d_sketch);
            if (sketch == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return null;
            }

            var sketchDef = (ksSketchDefinition)sketch.GetDefinition();
            if (sketchDef == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return null;
            }

            var basePlane = GetBasePlane(doc3DPart);
            if (basePlane == null)
            {
                LastErrorCode = ErrorCodes.ArgumentNull;
                return null;
            }

            sketchDef.SetPlane(basePlane);
            if (sketch.Create() != true)
            {
                LastErrorCode = ErrorCodes.EntityCreateError;
                return null;
            }

            _sketchDef = sketchDef;

            return sketch;
        }

        /// <summary>
        /// Get datum plane along axis or get already set datum plane
        /// </summary>
        /// <param name="doc3DPart">Part of 3D document (detail in build)</param>
        /// <returns>Reference plane or axis reference plane already set</returns>
        private ksEntity GetBasePlane(ksPart doc3DPart)
        {
            ksEntity basePlane = null;

            if (_basePlane != null)
            {
                basePlane = _basePlane;
            }
            else
            {
                basePlane = (ksEntity)doc3DPart.GetDefaultEntity((short)_basePlaneAxis);
            }

            return basePlane;
        }
    }
}
