//-----------------------------------------------------------------------------
//! \file   CommandBehavior.cs
//! \brief  An extended version of "AttachedCommandBehavior"
//
//          The original, non-extended version by Marlon Grech:
//          http://marlongrech.wordpress.com/2008/12/13/attachedcommandbehavior-v2-aka-acb/
//
//-----------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;

namespace Cider_x64.Helpers.AttachedCommandBehavior
{
    /// <summary>
    /// Interface for generic filtering of events according to their EventArgs
    /// </summary>
    public interface IEventFilter
    {
        /// <summary>
        /// IsMatchingEvent
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="eventArgs">event args</param>
        bool IsMatchingEvent(object sender, object eventArgs);
    }

    /// <summary>
    /// An "event filter" class which lets through only mouse events of the _left_ mouse button
    /// </summary>
    class EventFilter_LeftMouseButton : IEventFilter
    {
        // IEventFilter
        public bool IsMatchingEvent(object sender, object eventArgs)
        {
            return (eventArgs is MouseButtonEventArgs && (eventArgs as MouseButtonEventArgs).ChangedButton == MouseButton.Left);
        }
    }

    /// <summary>
    /// An "event filter" class which lets through only mouse events of the _left_ mouse button without Ctrl/Shift pressed
    /// </summary>
    class EventFilter_LeftMouseButtonNoCtrlShift : IEventFilter
    {
        // IEventFilter
        public bool IsMatchingEvent(object sender, object eventArgs)
        {
            return (eventArgs is MouseButtonEventArgs && (eventArgs as MouseButtonEventArgs).ChangedButton == MouseButton.Left &&
                    !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));
        }
    }

    /// <summary>
    /// An "event filter" class which lets through only key events of the "Enter" key
    /// </summary>
    class EventFilter_EnterKey : IEventFilter
    {
        // IEventFilter
        public bool IsMatchingEvent(object sender, object eventArgs)
        {
            return (eventArgs is KeyEventArgs && (eventArgs as KeyEventArgs).Key == Key.Enter);
        }
    }

    /// <summary>
    /// Defines the attached properties to create a CommandBehaviorBinding
    /// </summary>
    public class CommandBehavior
    {
        #region Behavior

        /// <summary>
        /// Behavior Attached Dependency Property
        /// </summary>
        private static readonly DependencyProperty BehaviorProperty =
            DependencyProperty.RegisterAttached("Behavior", typeof(CommandBehaviorBinding), typeof(CommandBehavior),
                new FrameworkPropertyMetadata((CommandBehaviorBinding)null));

        /// <summary>
        /// Gets the Behavior property. 
        /// </summary>
        private static CommandBehaviorBinding GetBehavior(DependencyObject d)
        {
            return (CommandBehaviorBinding)d.GetValue(BehaviorProperty);
        }

        /// <summary>
        /// Sets the Behavior property.  
        /// </summary>
        private static void SetBehavior(DependencyObject d, CommandBehaviorBinding value)
        {
            d.SetValue(BehaviorProperty, value);
        }

        #endregion

        #region Command

        /// <summary>
        /// Command Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(CommandBehavior),
                new FrameworkPropertyMetadata((ICommand)null,
                    new PropertyChangedCallback(OnCommandChanged)));

        /// <summary>
        /// Gets the Command property.  
        /// </summary>
        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        /// <summary>
        /// Sets the Command property. 
        /// </summary>
        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Handles changes to the Command property.
        /// </summary>
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandBehaviorBinding binding = FetchOrCreateBinding(d);
            binding.Command = (ICommand)e.NewValue;
        }

        #endregion

        #region Action

        /// <summary>
        /// Action Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.RegisterAttached("Action", typeof(Action<object>), typeof(CommandBehavior),
                new FrameworkPropertyMetadata((Action<object>)null,
                    new PropertyChangedCallback(OnActionChanged)));

        /// <summary>
        /// Gets the Action property.  
        /// </summary>
        public static Action<object> GetAction(DependencyObject d)
        {
            return (Action<object>)d.GetValue(ActionProperty);
        }

        /// <summary>
        /// Sets the Action property. 
        /// </summary>
        public static void SetAction(DependencyObject d, Action<object> value)
        {
            d.SetValue(ActionProperty, value);
        }

        /// <summary>
        /// Handles changes to the Action property.
        /// </summary>
        private static void OnActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandBehaviorBinding binding = FetchOrCreateBinding(d);
            binding.Action = (Action<object>)e.NewValue;
        }

        #endregion

        #region CommandParameter

        /// <summary>
        /// CommandParameter Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(CommandBehavior),
                new FrameworkPropertyMetadata((object)null,
                    new PropertyChangedCallback(OnCommandParameterChanged)));

        /// <summary>
        /// Gets the CommandParameter property.  
        /// </summary>
        public static object GetCommandParameter(DependencyObject d)
        {
            return (object)d.GetValue(CommandParameterProperty);
        }

        /// <summary>
        /// Sets the CommandParameter property. 
        /// </summary>
        public static void SetCommandParameter(DependencyObject d, object value)
        {
            d.SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Handles changes to the CommandParameter property.
        /// </summary>
        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandBehaviorBinding binding = FetchOrCreateBinding(d);
            binding.CommandParameter = e.NewValue;
        }

        #endregion

        #region Event

        /// <summary>
        /// Event Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty EventProperty =
            DependencyProperty.RegisterAttached("Event", typeof(string), typeof(CommandBehavior),
                new FrameworkPropertyMetadata((string)String.Empty,
                    new PropertyChangedCallback(OnEventChanged)));

        /// <summary>
        /// Gets the Event property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static string GetEvent(DependencyObject d)
        {
            return (string)d.GetValue(EventProperty);
        }

        /// <summary>
        /// Sets the Event property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetEvent(DependencyObject d, string value)
        {
            d.SetValue(EventProperty, value);
        }

        /// <summary>
        /// Handles changes to the Event property.
        /// </summary>
        private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandBehaviorBinding binding = FetchOrCreateBinding(d);
            //check if the Event is set. If yes we need to rebind the Command to the new event and unregister the old one
            if (binding.Event != null && binding.Owner != null)
                binding.Dispose();
            //bind the new event to the command
            binding.BindEvent(d, e.NewValue.ToString());
        }

        #endregion

        #region EventFilter

        private static EventFilter_LeftMouseButton _leftMouseButtonFilter;
        /// <summary>
        /// Convenience property for accessing an EventFilter_LeftMouseButton instance from XAML
        /// </summary>
        public static IEventFilter LeftMouseButtonFilter
        {
            get
            {
                if (_leftMouseButtonFilter == null)
                    _leftMouseButtonFilter = new EventFilter_LeftMouseButton();
                return _leftMouseButtonFilter;
            }
        }

        private static EventFilter_LeftMouseButtonNoCtrlShift _leftMouseButtonNoCtrlShiftFilter;
        /// <summary>
        /// Convenience property for accessing an EventFilter_LeftMouseButtonNoCtrlShift instance from XAML
        /// </summary>
        public static IEventFilter LeftMouseButtonNoCtrlShiftFilter
        {
            get
            {
                if (_leftMouseButtonNoCtrlShiftFilter == null)
                    _leftMouseButtonNoCtrlShiftFilter = new EventFilter_LeftMouseButtonNoCtrlShift();
                return _leftMouseButtonNoCtrlShiftFilter;
            }
        }

        private static EventFilter_EnterKey _enterKeyFilter;
        /// <summary>
        /// Convenience property for accessing an EventFilter_EnterKey instance from XAML
        /// </summary>
        public static IEventFilter EnterKeyFilter
        {
            get
            {
                if (_enterKeyFilter == null)
                    _enterKeyFilter = new EventFilter_EnterKey();
                return _enterKeyFilter;
            }
        }

        /// <summary>
        /// Gets the "EventFilter" attached property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEventFilter GetEventFilter(DependencyObject obj)
        {
            return (IEventFilter)obj.GetValue(EventFilterProperty);
        }

        /// <summary>
        /// Sets the "EventFilter" attached property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetEventFilter(DependencyObject obj, IEventFilter value)
        {
            obj.SetValue(EventFilterProperty, value);
        }

        /// <summary>
        /// Attached property "EventFilter"
        /// </summary>
        public static readonly DependencyProperty EventFilterProperty =
            DependencyProperty.RegisterAttached("EventFilter", typeof(IEventFilter), typeof(CommandBehavior), new UIPropertyMetadata(new PropertyChangedCallback(OnEventFilterChanged)));

        /// <summary>
        /// Change event handler for the "EventFilter" attached property
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnEventFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandBehaviorBinding binding = FetchOrCreateBinding(d);
            binding.EventFilter = e.NewValue;
        }

        #endregion

        #region Helpers
        //tries to get a CommandBehaviorBinding from the element. Creates a new instance if there is not one attached
        private static CommandBehaviorBinding FetchOrCreateBinding(DependencyObject d)
        {
            CommandBehaviorBinding binding = CommandBehavior.GetBehavior(d);
            if (binding == null)
            {
                binding = new CommandBehaviorBinding();
                CommandBehavior.SetBehavior(d, binding);
            }
            return binding;
        }
        #endregion

    }

}
