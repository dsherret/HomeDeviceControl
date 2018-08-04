using LightControl.Core.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LightControl
{
    public class HomeState : IEquatable<HomeState>
    {
        public double SunAltitude { get; set; }
        public DateTime CurrentTime { get; set; }

        private SunRoom _sunRoom;
        public SunRoom SunRoom
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
        private HomeState _homeState;

        public bool IsComputerOn { get; set; }
        public int Luminance { get; set; }
        public DateTime LastMotionDetected { get; set; }

        public bool IsDark => Luminance < 30 || _homeState.SunAltitude < 0;

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
            SunRoom = new SunRoom
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
