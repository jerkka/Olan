using Newtonsoft.Json;

namespace Olan {
    public static class JsonExtensions {
        #region Methods

        public static string ToJson<TFrom>(this TFrom @object) {
            return JsonConvert.SerializeObject(@object, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        #endregion
    }
}