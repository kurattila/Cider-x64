//-----------------------------------------------------------------------------
//! \file   ExecutionStrategy.cs
//! \brief  Part of AttachedCommandBehavior by Marlon Grech (a WPF Disciple)
//
//          http://marlongrech.wordpress.com/2008/12/13/attachedcommandbehavior-v2-aka-acb/
//
//-----------------------------------------------------------------------------

using System;

namespace Cider_x64.Helpers.AttachedCommandBehavior
{
    /// <summary>
    /// Defines the interface for a strategy of execution for the CommandBehaviorBinding
    /// </summary>
    public interface IExecutionStrategy
    {
        /// <summary>
        /// Gets or sets the Behavior that we execute this strategy
        /// </summary>
        CommandBehaviorBinding Behavior { get; set; }

        /// <summary>
        /// Executes according to the strategy type
        /// </summary>
        /// <param name="parameter">The parameter to be used in the execution</param>
        void Execute(object parameter);
    }

    /// <summary>
    /// Executes a command 
    /// </summary>
    public class CommandExecutionStrategy : IExecutionStrategy
    {
        #region IExecutionStrategy Members
        /// <summary>
        /// Gets or sets the Behavior that we execute this strategy
        /// </summary>
        public CommandBehaviorBinding Behavior { get; set; }

        /// <summary>
        /// Executes the Command that is stored in the CommandProperty of the CommandExecution
        /// </summary>
        /// <param name="parameter">The parameter for the command</param>
        public void Execute(object parameter)
        {
            if (Behavior == null)
                throw new InvalidOperationException("Behavior property cannot be null when executing a strategy");

            if (Behavior.Command.CanExecute(Behavior.CommandParameter))
                Behavior.Command.Execute(Behavior.CommandParameter);
        }

        #endregion
    }

    /// <summary>
    /// executes a delegate
    /// </summary>
    public class ActionExecutionStrategy : IExecutionStrategy
    {

        #region IExecutionStrategy Members

        /// <summary>
        /// Gets or sets the Behavior that we execute this strategy
        /// </summary>
        public CommandBehaviorBinding Behavior { get; set; }

        /// <summary>
        /// Executes an Action delegate
        /// </summary>
        /// <param name="parameter">The parameter to pass to the Action</param>
        public void Execute(object parameter)
        {
            Behavior.Action(parameter);
        }

        #endregion
    }

}
