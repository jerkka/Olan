using System;
using System.Collections.Generic;

namespace Olan {
    public sealed class RunStats<TStatus> {
        #region Status enum

        public enum Status {
            None = 0,
            Succeeded = 3,
            Failed = 4,
            Running = 5,
            Blacklisted = 9,
            ExceptionThrow = 12,
            Timeout = 13
        }

        #endregion
        #region Fields

        private Dictionary<DateTime, RunStats<TStatus>> _all;

        #endregion
        #region Constructors

        public RunStats() { }

        public RunStats(double lastExecutionTime, Exception lastException, TStatus lastStatus, string lastType = null) {
            LastExecutionTime = lastExecutionTime;
            LastException = lastException;
            LastStatus = lastStatus;
            LastTip = lastType ?? string.Empty;
        }

        #endregion
        #region Properties

        public Dictionary<DateTime, RunStats<TStatus>> All => _all ?? (_all = new Dictionary<DateTime, RunStats<TStatus>>());

        public Exception LastException { get; set; }
        public double LastExecutionTime { get; set; }
        public TStatus LastStatus { get; set; }
        public string LastTip { get; set; }

        #endregion
        #region Methods

        public RunStats<TStatus> Add() {
            return Add(this);
        }

        public RunStats<TStatus> Add(RunStats<TStatus> stats) {
            LastException = stats.LastException;
            LastExecutionTime = stats.LastExecutionTime;
            LastStatus = stats.LastStatus;
            LastTip = stats.LastTip;
            All.Add(DateTime.Now, stats);
            return this;
        }

        public RunStats<TStatus> Clone() {
            return new RunStats<TStatus>(LastExecutionTime, LastException, LastStatus, LastTip);
        }

        #endregion
    }
}