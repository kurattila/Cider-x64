//-----------------------------------------------------------------------------
//! \file   SimpleCommand.cs
//! \brief  Part of AttachedCommandBehavior by Marlon Grech (a WPF Disciple)
//
//          http://marlongrech.wordpress.com/2008/12/13/attachedcommandbehavior-v2-aka-acb/
//
//-----------------------------------------------------------------------------

using System;
using System.Windows.Input;

namespace Cider_x64.Helpers.AttachedCommandBehavior
{
    /// <summary>
    /// Implements the ICommand and wraps up all the verbose stuff so that you can just pass 2 delegates 1 for the CanExecute and one for the Execute
    /// </summary>
    public class SimpleCommand : ICommand
    {
        /// <summary>
        /// Gets or sets the Predicate to execute when the CanExecute of the command gets called
        /// </summary>
        public Predicate<object> CanExecuteDelegate { get; set; }

        /// <summary>
        /// Gets or sets the action to be called when the Execute method of the command gets called
        /// </summary>
        public Action<object> ExecuteDelegate { get; set; }

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="execute">action</param>
        public SimpleCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="execute">action</param>
        /// <param name="canExecute">predicate</param>
        public SimpleCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            ExecuteDelegate = execute;
            CanExecuteDelegate = canExecute;
        }
        #endregion // Constructors

        #region ICommand Members

        /// <summary>
        /// Checks if the command Execute method can run
        /// </summary>
        /// <param name="parameter">THe command parameter to be passed</param>
        /// <returns>Returns true if the command can execute. By default true is returned so that if the user of SimpleCommand does not specify a CanExecuteCommand delegate the command still executes.</returns>
        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true;// if there is no can execute default to true
        }

        /// <summary>
        /// CanExecuteChanged event
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Executes the actual command
        /// </summary>
        /// <param name="parameter">THe command parameter to be passed</param>
        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }

        #endregion
    }
}
