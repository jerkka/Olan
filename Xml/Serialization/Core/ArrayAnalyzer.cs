using System;
using System.Collections.Generic;
using System.Linq;

namespace Olan.Xml.Serialization.Core {
    /// <summary>
    ///   Gives information about actually analysed array (from the constructor)
    /// </summary>
    public class ArrayAnalyzer {
        #region Fields

        private readonly object _array;
        private readonly ArrayInfo _arrayInfo;
        private IList<int[]> _indexes;

        #endregion
        #region Constructors

        ///<summary>
        ///</summary>
        ///<param name = "array"></param>
        public ArrayAnalyzer(object array) {
            _array = array;
            var type = array.GetType();
            _arrayInfo = GetArrayInfo(type);
        }

        #endregion
        #region Properties

        /// <summary>
        ///   Contains extended information about the current array
        /// </summary>
        public ArrayInfo ArrayInfo => _arrayInfo;

        #endregion
        #region Methods

        /// <summary>
        ///   How many dimensions. There can be at least 1
        /// </summary>
        /// <returns></returns>
        private int GetRank(Type arrayType) {
            return arrayType.GetArrayRank();
        }

        /// <summary>
        ///   How many items in one dimension
        /// </summary>
        /// <param name = "dimension">0-based</param>
        /// <returns></returns>
        /// <param name="arrayType"></param>
        private int GetLength(int dimension, Type arrayType) {
            var methodInfo = arrayType.GetMethod("GetLength");
            var length = (int)methodInfo.Invoke(_array, new object[] {
                dimension
            });
            return length;
        }

        /// <summary>
        ///   Lower index of an array. Default is 0.
        /// </summary>
        /// <param name = "dimension">0-based</param>
        /// <returns></returns>
        /// <param name="arrayType"></param>
        private int GetLowerBound(int dimension, Type arrayType) {
            return GetBound("GetLowerBound", dimension, arrayType);
        }

//        private int getUpperBound(int dimension)
//        {
        // Not used, as UpperBound is equal LowerBound+Length
//            return getBound("GetUpperBound", dimension);
//        }

        private int GetBound(string methodName, int dimension, Type arrayType) {
            var methodInfo = arrayType.GetMethod(methodName);
            var bound = (int)methodInfo.Invoke(_array, new object[] {
                dimension
            });
            return bound;
        }

        private ArrayInfo GetArrayInfo(Type arrayType) {
            // Caching is innacceptable, as an array of type string can have different bounds

            var info = new ArrayInfo();

            // Fill the dimension infos
            for (var dimension = 0; dimension < GetRank(arrayType); dimension++) {
                var dimensionInfo = new DimensionInfo {
                    Length = GetLength(dimension, arrayType), LowerBound = GetLowerBound(dimension, arrayType)
                };
                info.DimensionInfos.Add(dimensionInfo);
            }

            return info;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public IEnumerable<int[]> GetIndexes() {
            if (_indexes == null) {
                _indexes = new List<int[]>();
                ForEach(AddIndexes);
            }

            return _indexes;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public IEnumerable<object> GetValues() {
            return GetIndexes().Select(indexSet => ((Array)_array).GetValue(indexSet));
        }

        private void AddIndexes(int[] obj) {
            _indexes.Add(obj);
        }

        ///<summary>
        ///</summary>
        ///<param name = "action"></param>
        public void ForEach(Action<int[]> action) {
            var dimensionInfo = _arrayInfo.DimensionInfos[0];
            for (var index = dimensionInfo.LowerBound; index < dimensionInfo.LowerBound + dimensionInfo.Length; index++) {
                var result = new List<int> {
                    index
                };

                // Adding the first coordinate

                if (_arrayInfo.DimensionInfos.Count < 2) {
                    // only one dimension
                    action.Invoke(result.ToArray());
                    continue;
                }

                // further dimensions
                forEach(_arrayInfo.DimensionInfos, 1, result, action);
            }
        }

        /// <summary>
        ///   This functiona will be recursively used
        /// </summary>
        /// <param name = "dimensionInfos"></param>
        /// <param name = "dimension"></param>
        /// <param name = "coordinates"></param>
        /// <param name = "action"></param>
        private void forEach(IList<DimensionInfo> dimensionInfos, int dimension, IEnumerable<int> coordinates, Action<int[]> action) {
            var dimensionInfo = dimensionInfos[dimension];
            for (var index = dimensionInfo.LowerBound; index < dimensionInfo.LowerBound + dimensionInfo.Length; index++) {
                var result = new List<int>(coordinates) {
                    index
                };

                // Adding the first coordinate

                if (dimension == _arrayInfo.DimensionInfos.Count - 1) {
                    // This is the last dimension
                    action.Invoke(result.ToArray());
                    continue;
                }

                // Further dimensions
                forEach(_arrayInfo.DimensionInfos, dimension + 1, result, action);
            }
        }

        #endregion
    }

    /// <summary>
    ///   Contain info about array (i.e. how many dimensions, lower/upper bounds)
    /// </summary>
    public sealed class ArrayInfo {
        #region Fields

        private IList<DimensionInfo> _dimensionInfos;

        #endregion
        #region Properties

        ///<summary>
        ///</summary>
        public IList<DimensionInfo> DimensionInfos {
            get { return _dimensionInfos ?? (_dimensionInfos = new List<DimensionInfo>()); }
            set { _dimensionInfos = value; }
        }

        #endregion
    }
}