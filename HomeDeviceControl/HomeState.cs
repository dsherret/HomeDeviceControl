using HomeDeviceControl.Core.Utils;
using System;
using System.Threading.Tasks;

namespace HomeDeviceControl
{
    public class HomeState : IEquatable<HomeState>
    {
        public double SunAltitude { get; set; }
        public DateTime CurrentTime { get; set; }

        private Sunroom _sunRoom;
        public Sunroom Sunroom
        {
            get => _sunRoom;
            set
            {
                _sunRoom = value;
                value.SetHomeState(this);
            }
        }

        public HomeState Clone()
        {
            return new HomeState
            {
                SunAltitude = SunAltitude,
                CurrentTime = CurrentTime,
                Sunroom = Sunroom.Clone()
            };
        }

        public bool Equals(HomeState other)
        {
            return SunAltitude == other.SunAltitude
                && CurrentTime == other.CurrentTime
                && Sunroom.Equals(other.Sunroom);
        }
    }

    public class Sunroom : IEquatable<Sunroom>
    {
        private HomeState _homeState;

        public bool IsComputerOn { get; set; }
        public int Luminance { get; set; }
        public DateTime LastMotionDetected { get; set; }

        public bool IsDark => Luminance < Settings.Default.SunroomMotionDarkLuminance
            || _homeState.SunAltitude < Settings.Default.SunroomMotionDarkSunAltitude;

        public Sunroom Clone()
        {
            return new Sunroom
            {
                IsComputerOn = IsComputerOn,
                Luminance = Luminance,
                LastMotionDetected = LastMotionDetected
            };
        }

        public bool Equals(Sunroom other)
        {
            return IsComputerOn == other.IsComputerOn
                && Luminance == other.Luminance
                && LastMotionDetected == other.LastMotionDetected;
        }

        internal void SetHomeState(HomeState homeState)
        {
            _homeState = homeState;
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

        private readonly AsyncEventContainer<StateChangedEventArgs> _stateChangedEventContainer = new AsyncEventContainer<StateChangedEventArgs>();
        private readonly AsyncCoordinationLock _coordinationLock;
        private readonly object _lock = new object();
        private HomeState _state = new HomeState
        {
            SunAltitude = 100,
            Sunroom = new Sunroom
            {
                IsComputerOn = false,
                LastMotionDetected = DateTime.Now.AddDays(-1),
                Luminance = 100
            },
            CurrentTime = DateTime.Now
        };

        public HomeStateContainer(AsyncCoordinationLock coordinationLock)
        {
            _coordinationLock = coordinationLock;
        }

        public HomeState GetCurrentState()
        {
            lock (_lock)
                return _state;
        }

        public async void OnStateUpdated(Func<StateChangedEventArgs, Task> action)
        {
            await _coordinationLock.DoActionAsync(() =>
            {
                _stateChangedEventContainer.Add(action);
            });
        }

        public async void UpdateState(Action<HomeState> update)
        {
            await _coordinationLock.DoActionAsync(async () =>
            {
                HomeState previousState;
                HomeState newState;
                lock (_lock)
                {
                    previousState = _state;
                    newState = previousState.Clone();
                    update(newState);
                    _state = newState;
                }

                if (!previousState.Equals(newState))
                    await _stateChangedEventContainer.InvokeAsync(new StateChangedEventArgs(previousState, newState));
            });
        }
    }
}
