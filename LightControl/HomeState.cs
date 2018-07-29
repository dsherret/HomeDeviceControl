using System;

namespace LightControl
{
    public class HomeState : IEquatable<HomeState>
    {
        public double SunAltitude { get; set; }
        public DateTime CurrentTime { get; set; }

        public SunRoom SunRoom { get; set; }

        public HomeState Clone()
        {
            return new HomeState
            {
                SunAltitude = SunAltitude,
                CurrentTime = CurrentTime,
                SunRoom = SunRoom.Clone()
            };
        }

        public bool Equals(HomeState other)
        {
            return SunAltitude == other.SunAltitude
                && CurrentTime == other.CurrentTime
                && SunRoom.Equals(other.SunRoom);
        }
    }

    public class SunRoom : IEquatable<SunRoom>
    {
        public bool IsComputerOn { get; set; }
        public int Luminance { get; set; }
        public DateTime LastMotionDetected { get; set; }

        public SunRoom Clone()
        {
            return new SunRoom
            {
                IsComputerOn = IsComputerOn,
                Luminance = Luminance,
                LastMotionDetected = LastMotionDetected
            };
        }

        public bool Equals(SunRoom other)
        {
            return IsComputerOn == other.IsComputerOn
                && Luminance == other.Luminance
                && LastMotionDetected == other.LastMotionDetected;
        }
    }

    public class HomeStateContainer
    {
        public class StateChangedEventArgs : EventArgs
        {
            public StateChangedEventArgs(HomeState previousState, HomeState newState)
            {
                OldState = previousState;
                NewState = newState;
            }

            public HomeState OldState { get; }
            public HomeState NewState { get; }
        }

        private HomeState _state = new HomeState
        {
            SunAltitude = 100,
            SunRoom = new SunRoom
            {
                IsComputerOn = false,
                LastMotionDetected = DateTime.Now.AddDays(-1)
            },
            CurrentTime = DateTime.Now
        };

        public event EventHandler<StateChangedEventArgs> StateUpdated;

        public void UpdateState(Action<HomeState> update)
        {
            var previousState = _state;

            _state = _state.Clone();
            update(_state);

            if (!previousState.Equals(_state))
                StateUpdated?.Invoke(this, new StateChangedEventArgs(previousState, _state));
        }
    }
}
