﻿using System;
using System.Runtime.ExceptionServices;

namespace Olan.AsyncEx.Internal.PlatformEnlightenment {
    public static class ExceptionEnlightenment {
        #region Methods

        public static Exception PrepareForRethrow(Exception exception) {
            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception;
        }

        #endregion
    }
}