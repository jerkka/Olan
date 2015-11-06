using System.Threading.Tasks;

namespace Olan.AsyncEx.Internal.PlatformEnlightenment {
    public static class AsyncEnlightenment {
        #region Methods

        public static TaskCreationOptions AddDenyChildAttach(TaskCreationOptions options) {
            return options | TaskCreationOptions.DenyChildAttach;
        }

        #endregion
    }
}