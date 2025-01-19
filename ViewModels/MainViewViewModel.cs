using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Civil3DArbitraryCoordinate.Enums;
using Civil3DArbitraryCoordinate.Models;
using Civil3DArbitraryCoordinate.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Civil3DArbitraryCoordinate.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            try
            {
                SelectPipeCommand = new DelegateCommand(OnSelectPipeCommand);
                SelectPointCommand = new DelegateCommand(OnSelectPointCommand, OnSelectPointCommandCanExecute);
                SelectLabelsToUpdateCommand = new DelegateCommand(OnSelectLabelsToUpdateCommand);

                List<ObjectId> pipeLabelStyleIds = CivilDocumentService.CivilDocument.Styles.LabelStyles.PipeLabelStyles.PlanProfileLabelStyles.ToList();

                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    PipeLabelStyles = ObjectIdUtils<LabelStyle>.ConvertToType(pipeLabelStyleIds, transaction).OrderBy(item => item.Name).ToList();
                    SelectedPipeLabelStyle = string.IsNullOrEmpty(Properties.Settings.Default.LabelStyleName) ? PipeLabelStyles.FirstOrDefault() : PipeLabelStyles.FirstOrDefault(item => item.Name == Properties.Settings.Default.LabelStyleName);
                    transaction.Commit();
                }

                ElevationTypes = new List<ElevationType> { ElevationType.OuterBottom, ElevationType.InnerBottom, ElevationType.Center, ElevationType.InnerTop, ElevationType.OuterTop };
                SelectedElevationType = string.IsNullOrEmpty(Properties.Settings.Default.SelectedElevationType) ? ElevationType.Center : Enum.Parse<ElevationType>(Properties.Settings.Default.SelectedElevationType);

                LabelVariable = string.IsNullOrEmpty(Properties.Settings.Default.LabelVariable) ? "$ELEVATION$" : Properties.Settings.Default.LabelVariable;

                RoundingItems = new List<string> { "0.0", "0.00", "0.000", "0.000" };
                SelectedRoundingItem = string.IsNullOrEmpty(Properties.Settings.Default.RoundingItem) ? RoundingItems.FirstOrDefault() : RoundingItems.FirstOrDefault(item => item == Properties.Settings.Default.RoundingItem);

                EventAggregator = eventAggregator;

                SelectPipeInfo = "Select Pipe";
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error");
            }
        }

        public List<ElevationType> ElevationTypes { get; } = new List<ElevationType>();

        public ElevationType SelectedElevationType
        {
            get
            {
                return selectedElevationType;
            }
            set
            {
                selectedElevationType = value;

                Properties.Settings.Default.SelectedElevationType = selectedElevationType.ToString();
                Properties.Settings.Default.Save();
                SelectPointCommand.RaiseCanExecuteChanged();

                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectPipeCommand { get; }
        public DelegateCommand SelectPointCommand { get; }
        public List<LabelStyle> PipeLabelStyles { get; set; } = new List<LabelStyle>();

        public LabelStyle SelectedPipeLabelStyle { get; set; }

        private string labelVariable;

        public string LabelVariable
        {
            get { return labelVariable; }
            set
            {
                labelVariable = value;

                Properties.Settings.Default.LabelVariable = labelVariable;
                Properties.Settings.Default.Save();
                SelectPointCommand.RaiseCanExecuteChanged();

                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectLabelsToUpdateCommand { get; }
        public string SelectPipeInfo
        {
            get
            {
                return selectPipeInfo;
            }
            set
            {
                selectPipeInfo = value;
                RaisePropertyChanged();
            }
        }

        public IEventAggregator EventAggregator { get; }

        private Pipe pipe;

        public Pipe Pipe
        {
            get
            {
                return pipe;
            }
            set
            {
                pipe = value;
                SelectPointCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged();
            }
        }

        private List<string> roundingItems;

        public List<string> RoundingItems
        {
            get { return roundingItems; }
            set
            {
                roundingItems = value;
                RaisePropertyChanged();
            }
        }

        private string selectedRoundingItem;
        private ElevationType selectedElevationType;
        private string selectPipeInfo;

        public string SelectedRoundingItem
        {
            get { return selectedRoundingItem; }
            set
            {
                selectedRoundingItem = value;
                Properties.Settings.Default.RoundingItem = selectedRoundingItem;
                Properties.Settings.Default.Save();

                SelectPointCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged();
            }
        }

        public int SelectedRoundingInt
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedRoundingItem))
                {
                    return 0;
                }

                string[] roundingSplitted = SelectedRoundingItem.Split('.');

                if (roundingSplitted.Length < 2)
                {
                    return 0;
                }

                string decimals = roundingSplitted[1];

                return decimals.ToCharArray().Count();
            }
        }

        public Point3d? Point { get; set; }




        private bool OnSelectPointCommandCanExecute()
        {
            return Pipe != null && !string.IsNullOrEmpty(LabelVariable) && SelectedPipeLabelStyle != null && !string.IsNullOrEmpty(SelectedRoundingItem);
        }

        private void OnSelectPointCommand()
        {
            try
            {
                if (Pipe == null || SelectedPipeLabelStyle == null || string.IsNullOrEmpty(SelectedRoundingItem) || string.IsNullOrEmpty(LabelVariable))
                {
                    return;
                }

                Point = PromptUtils.PromptPoint("Pick point to set coordinate");

                if (!Point.HasValue)
                {
                    return;
                }

                PipeArbitraryPoint pipeArbitraryPoint = PipeArbitraryPoint.Create(Pipe, new Point2d(Point.Value.X, Point.Value.Y));

                double elevationValue = pipeArbitraryPoint.GetElevationValue(SelectedRoundingInt, SelectedElevationType);

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        Pipe pipeOpened = transaction.GetObject(Pipe.Id, OpenMode.ForWrite, false, true) as Pipe;
                        ObjectId labelId = PipeLabel.Create(pipeOpened.Id, pipeArbitraryPoint.RatioFromStartPoint, SelectedPipeLabelStyle.ObjectId);
                        PipeLabel pipeLabel = transaction.GetObject(labelId, OpenMode.ForWrite, false, true) as PipeLabel;

                        ObjectIdCollection componentTextCollection = SelectedPipeLabelStyle.GetComponents(LabelStyleComponentType.Text);

                        pipeLabel.SetTextComponentOverride(transaction, LabelVariable, elevationValue.ToString(SelectedRoundingItem));

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error");
            }
        }

        private void OnSelectLabelsToUpdateCommand()
        {

        }

        private void OnSelectPipeCommand()
        {
            try
            {
                Pipe = SelectionUtils.GetElement<Pipe>("Select pipes to label");
                SelectPipeInfo = $"Selected: {Pipe.Name} from {pipe.NetworkName}";
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error");
            }
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
