using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Olan.UI {
    public class RelayCommand : ICommand {
        #region Fields

        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;
        private readonly Action _handler;

        #endregion
        #region Constructors

        public RelayCommand(Action execute) {
            _handler = execute;
        }

        public RelayCommand(Action<object> execute)
            : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion
        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter) {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) {
            if (_handler != null) {
                _handler();
                return;
            }

            _execute(parameter);
        }

        #endregion
    }
}