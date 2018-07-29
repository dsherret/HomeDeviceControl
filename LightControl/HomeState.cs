using System;

namespace LightControl
{
    public struct HomeState
    {
        public double SunAltitude { get; set; }
        public bool IsComputerOn { get; set; }
    }

    public class HomeStateContainer
    {
        public class StateChangedEventArgs : EventArgs
        {
            public StateChangedEventArgs(HomeState previousState, HomeState newState)
            {
                PreviousState = previousState;
                NewState = newState;
            }

            public HomeState PreviousState { get; }
            public HomeState NewState { get; }
        }

        private HomeState _state = new HomeState
        {
            SunAltitude = 100,
            IsComputerOn = false
        };

        public event EventHandler<StateChangedEventArgs> StateUpdated;

        public void UpdateState(Func<HomeState, HomeState> update)
        {
            var previousState = _state;
            var newState = update(previousState);
            _state = newState;
            StateUpdated?.Invoke(this, new StateChangedEventArgs(previousState, newState));
        }
    }
}
