using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Civil3DArbitraryCoordinate.Enums;
using Civil3DArbitraryCoordinate.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Civil3DArbitraryCoordinate.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        public List<ElevationType> ElevationTypes { get; } = new List<ElevationType>();

        public ElevationType SelectedElevationType { get; set; }

        public DelegateCommand SelectPipesCommand { get; }

        public List<LabelStyle> PipeLabelStyles { get; set; } = new List<LabelStyle>();

        public LabelStyle SelectedPipeLabelStyle { get; set; }

        private string labelVariable;

        public string LabelVariable
        {
            get { return labelVariable; }
            set
            {
                labelVariable = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectLabelsToUpdateCommand { get; }
        public IEventAggregator EventAggregator { get; }
        public List<Pipe> Pipes { get; private set; } = new List<Pipe>();

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            try
            {
                List<ObjectId> pipeLabelStyleIds = CivilDocumentService.CivilDocument.Styles.LabelStyles.PipeLabelStyles.PlanProfileLabelStyles.ToList();

                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    PipeLabelStyles = ObjectIdUtils<LabelStyle>.ConvertToType(pipeLabelStyleIds, transaction).OrderBy(item => item.Name).ToList();
                    SelectedPipeLabelStyle = PipeLabelStyles.FirstOrDefault();
                    transaction.Commit();
                }

                ElevationTypes = new List<ElevationType> { ElevationType.OuterBottom, ElevationType.InnerBottom, ElevationType.Center, ElevationType.InnerTop, ElevationType.OuterTop };
                SelectedElevationType = ElevationTypes.FirstOrDefault();
                SelectPipesCommand = new DelegateCommand(OnSelectPipesCommand);
                EventAggregator = eventAggregator;
                LabelVariable = "$COORDINATE$";

                SelectLabelsToUpdateCommand = new DelegateCommand(OnSelectLabelsToUpdateCommand);
            }
            catch (Exception)
            {

            }
        }

        private void OnSelectLabelsToUpdateCommand()
        {
            throw new NotImplementedException();
        }

        private void OnSelectPipesCommand()
        {
            Pipes = SelectionUtils.GetElements<Pipe>("Select pipes to label");
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
