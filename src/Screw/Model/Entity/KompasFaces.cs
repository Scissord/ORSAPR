using System;
using System.Linq;
using System.Collections.Generic;
using Kompas6API5;
using Kompas6Constants3D;
using Screw.Validator;

namespace Screw.Model.Entity
{
    public static class KompasFaces
    {
        /// <summary>
        /// Base face area state: 
        /// higher than parralel base face 
        /// or lower than the latter
        /// </summary>
        public enum BaseFaceAreaState
        {
            BaseFaceAreaHigher,
            BaseFaceAreaLower
        }

        /// <summary>
        /// Get face entity by index
        /// </summary>
        /// <param name="_doc3DPart">Kompas part of 3D document</param>
        /// <param name="index">Returned face index</param>
        /// <returns>Face entity by index</returns>
        public static ksEntity GetFaceEntityByIndex(ksPart _doc3DPart, int index)
        {
            var faceCollection = (ksEntityCollection)_doc3DPart.EntityCollection(
                (short)Obj3dType.o3d_face);
            var face = (ksEntity)faceCollection.GetByIndex(index);

            if (face == null)
            {
                return null;
            }
            else
            {
                return face;
            }
        }

        /// <summary>
        /// Get face definition by index
        /// </summary>
        /// <param name="_doc3DPart">Kompas part of 3D document</param>
        /// <param name="index">Returned face index</param>
        /// <returns>Face definition by index</returns>
        public static ksFaceDefinition GetFaceByIndex(ksPart _doc3DPart, int index)
        {
            var faceCollection = (ksEntityCollection)_doc3DPart.EntityCollection(
                (short)Obj3dType.o3d_face);
            var faceEntity = (ksEntity)faceCollection.GetByIndex(index);
            var face = (ksFaceDefinition)faceEntity.GetDefinition();

            if (face == null)
            {
                return null;
            }
            else
            {
                return face;
            }
        }

        /// <summary>
        /// Получить индексы базовой плоскости цилиндра по индексам граней цилиндра
        /// внутри коллекции граней деталей
        /// </summary>
        /// <param name="_doc3DPart">Document 3D part, представляет деталь</param>
        /// <param name="startIndex">Start index of faces in faces collection</param>
        /// <param name="endIndex">End index of faces in faces collection</param>
        /// <param name="outFirstIndex">First base plane index</param>
        /// <param name="outSecondIndex">Second base plane index</param>
        public static void GetCylinderBasePlaneIndexes(ksPart _doc3DPart,
            int startIndex, int endIndex, out int outFirstIndex, out int outSecondIndex)
        {
            var faceCollection = (ksEntityCollection)_doc3DPart.EntityCollection(
                (short)Obj3dType.o3d_face);

            if (faceCollection == null
                || !DoubleValidator.Validate(startIndex)
                || !DoubleValidator.Validate(endIndex)
            )
            {
                outFirstIndex = outSecondIndex = -1;
                return;
            }

            bool isFirstIndexSet = false;

            int firstIndex = -1;
            int secondIndex = -1;

            for (int i = startIndex - 1; i < endIndex; i++)
            {
                uint ST_MIX_SM = 0x0;   
                var entity = (ksEntity)faceCollection.GetByIndex(i);
                var def = (ksFaceDefinition)entity.GetDefinition();

                var area = Math.Round(def.GetArea(ST_MIX_SM), 10); 

                if (!def.IsCylinder() && isFirstIndexSet == false)
                {
                    isFirstIndexSet = true;
                    firstIndex = i;
                }
                else if (!def.IsCylinder() && isFirstIndexSet == true)
                {
                    secondIndex = i;
                }
            }

            outFirstIndex = firstIndex;
            outSecondIndex = secondIndex;

            return;
        }

        /// <summary>
        /// Вернуть грань, параллельную базовой грани
        /// </summary>
        /// Этот алгоритм использует области сбора граней в диапазоне
        /// от начального индекса до конечного индекса. 
        /// <param name="_doc3DPart">Kompas part of 3D document</param>
        /// <param name="startIndex">Face collection start index</param>
        /// <param name="endIndex">Face collection end index</param>
        /// <param name="outFirstIndex">First base plane index in faces collection</param>
        /// <param name="outSecondIndex">Second base plane index in faces collection</param>
        public static void GetRegPolyBasePlanesIndexes(ksPart _doc3DPart, int startIndex,
            int endIndex, out int outFirstIndex, out int outSecondIndex)
        {
            var faceCollection = (ksEntityCollection)_doc3DPart.EntityCollection(
                (short)Obj3dType.o3d_face);

            List<double> initList = new List<double>();
            List<double> unroundedList = new List<double>();
            List<double> uniqList = new List<double>();
            List<double> notUniqList = new List<double>();

            if (faceCollection == null
                || !DoubleValidator.Validate(startIndex)
                || !DoubleValidator.Validate(endIndex)
            )
            {
                outFirstIndex = outSecondIndex = -1;
                return;
            }

            int firstIndex = startIndex - 1;
            int secondIndex = startIndex - 1;

            for (int i = startIndex - 1; i < endIndex; i++)
            {
                uint ST_MIX_SM = 0x0;
                var entity = (ksEntity)faceCollection.GetByIndex(i);

                var def = (ksFaceDefinition)entity.GetDefinition();

                var area = def.GetArea(ST_MIX_SM);

                unroundedList.Add(area);
            }

            var minimalEpsilon = GetMinimalEspilonOfAreas(unroundedList);

            for (int i = 0, length = unroundedList.Count; i < length; i++)
            {
                initList.Add(Math.Round(unroundedList[i], minimalEpsilon));
            }

            for (int i = 0, length = initList.Count; i < length; i++)
            {

                if (!uniqList.Contains(initList[i]))
                {
                    if (!notUniqList.Contains(initList[i]))
                    {
                        uniqList.Add(initList[i]);
                    }
                }
                else
                {
                    uniqList.Remove(initList[i]);
                    notUniqList.Add(initList[i]);
                }
            }

            if (uniqList.Count == 2)
            {
                firstIndex += initList.IndexOf(uniqList[0]);
                secondIndex += initList.IndexOf(uniqList[1]);
            }

            else if (uniqList.Count == 1)
            {
                firstIndex += initList.IndexOf(uniqList[0]);
                secondIndex = -1;
            }

            else if (uniqList.Count == 0)
            {
                bool getFirstElement = false;

                for (int i = 0, count = initList.Count; i < count; i++)
                {
                    bool isTwoElements = (GetElementCountInList(initList, 
                        initList[i]) == 2);

                    if ((isTwoElements == true) && (getFirstElement == false))
                    {
                        firstIndex += i;
                        getFirstElement = true;
                    }
                    else if ((isTwoElements == true) && (getFirstElement == true))
                    {
                        secondIndex += i;
                        break;
                    }
                }
            }
            
            else
            {
                firstIndex = secondIndex = -1;
            }

            outFirstIndex = firstIndex;
            outSecondIndex = secondIndex;
        }

        /// <summary>
        /// Get minimal epsilon in unrounded areas list
        /// </summary>
        /// <param name="areas">Unrounded areas list</param>
        /// <returns>Minimal epsilon or -1 in case of error</returns>
        private static int GetMinimalEspilonOfAreas(List<double> areas)
        {
            var epsilons = new List<int>();

            for (int i = 0, length = areas.Count; i < length; i++)
            {
                epsilons.Add(GetEpsilonOfArea(areas[i]));
            }

            var epsilon = epsilons.Max();

            if (epsilon > 15)
            {
                epsilon = 15;
            }

            return epsilon;
        }

        /// <summary>
        /// Get epsilon to adequate round area
        /// </summary>
        /// <param name="area">Area of plane</param>
        /// <returns></returns>
        private static int GetEpsilonOfArea(double area)
        {
            var epsilon = 13;

            if (area > 1)
            {
                return epsilon - (int)Math.Floor(Math.Log10(area)) - 1;
            }

            else if (area < 1)
            {
                var decimals = 0;

                while (area < 1)
                {
                    area *= 10;
                    decimals++;
                }

                return epsilon + decimals - 1;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Получить базовую плоскость, параллельную _основной базовой плоскости.
        /// Разница между этими плоскостями заключается в том, 
        /// что базовая плоскость _main_ является выдавливаемой сущностью,
        /// а базовая плоскость _parallel_ выдавленной вытянутой сущностью.
        /// </summary>
        /// <param name="_doc3DPart">Kompas part of 3D document</param>
        /// <param name="faceIndex1">Base plane index 1</param>
        /// <param name="faceIndex2">Base plane index 2</param>
        /// <returns>Parallel base plane by base plane area 
        /// state and indexes of faces in detail faces collection</returns>
        public static ksEntity GetParallelBasePlane(ksPart _doc3DPart, 
            int faceIndex1, int faceIndex2, BaseFaceAreaState baseFaceAreaState)
        {
            if (faceIndex1 == -1)
            {
                return null;
            }

            var faceCollection = (ksEntityCollection)_doc3DPart.EntityCollection(
                (short)Obj3dType.o3d_face);

            var face1 = (ksEntity)faceCollection.GetByIndex(faceIndex1);
            var faceDefinition1 = (ksFaceDefinition)face1.GetDefinition();

            if (faceIndex2 == -1)
            {
                return face1;
            }

            var face2 = (ksEntity)faceCollection.GetByIndex(faceIndex2);
            var faceDefinition2 = (ksFaceDefinition)face2.GetDefinition();

            var face = (ksEntity)faceCollection.GetByIndex(0);

            uint SM = 0x0; 

            switch (baseFaceAreaState)
            {
                case BaseFaceAreaState.BaseFaceAreaHigher:
                    if (faceDefinition1.GetArea(SM) > faceDefinition2.GetArea(SM))
                    {
                        face = face2;
                    }
                    else
                    {
                        face = face1;
                    }
                    break;
                case BaseFaceAreaState.BaseFaceAreaLower:
                    if (faceDefinition1.GetArea(SM) < faceDefinition2.GetArea(SM))
                    {
                        face = face2;
                    }
                    else
                    {
                        face = face1;
                    }
                    break;
            }

            return face;
        }

        /// <summary>
        /// Draws the word "XYZ" on plane. 
        /// Эта функция используется для отладки других функций
        /// которые работают с эскизами.
        /// </summary>
        /// <param name="_doc3DPart">Kompas part of 3D document</param>
        /// <param name="plane">Selected plane</param>
        public static void DrawXyzOnPlane(ksPart _doc3DPart, ksEntity plane)
        {
            var xyz = new KompasSketch(_doc3DPart, plane);

            var xyzEdit = xyz.BeginEntityEdit();

            xyzEdit.ksLineSeg(0, 0, -5, -10, 1);        
            xyzEdit.ksLineSeg(0, -10, -5, 0, 1);       

            xyzEdit.ksLineSeg(-7, 0, -13, -10, 1);      
            xyzEdit.ksLineSeg(-7, -10, -10, -5, 1);    

            xyzEdit.ksLineSeg(-15, -10, -20, -10, 1);   
            xyzEdit.ksLineSeg(-20, -10, -15, 0, 1);     
            xyzEdit.ksLineSeg(-15, 0, -20, 0, 1);       

            xyz.EndEntityEdit();
        }

        /// <summary>
        ///Получить количество элементов в списке
        /// </summary>
        /// <param name="list">List of doubles</param>
        /// <param name="findElement">Element to find</param>
        /// <returns>Elements count in list</returns>
        private static int GetElementCountInList(List<double> list,
            double findElement)
        {
            int count = 0;

            foreach (var item in list)
            {
                if (item == findElement) count++;
            }

            return count;
        }
    }
}
