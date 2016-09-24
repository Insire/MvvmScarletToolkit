using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public class SceneViewModel : ViewModelBase<Scene>
    {
        public EventHandler<SceneChangedEvntArgs> SelectionChanged;

        private Scene _selectedItem;
        public Scene SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                var oldValue = _selectedItem;
                SetValue(ref _selectedItem, value, () => OnSelectionChanged(oldValue, value));
            }
        }

        public ICommand ForwardCommand { get; protected set; }
        public ICommand BackwardCommand { get; protected set; }

        public SceneViewModel()
        {
            InitializeProperties();
            InitializeCommands();
        }

        private void InitializeProperties()
        {

        }

        private void InitializeCommands()
        {
        }



        private void OnSelectionChanged(Scene oldScene, Scene newScene)
        {
            SelectionChanged?.Invoke(this, new SceneChangedEvntArgs { OldScene = oldScene, NewScene = newScene });
        }
    }
}
