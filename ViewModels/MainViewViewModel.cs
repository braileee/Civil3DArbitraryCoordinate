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
        public List<ElevationType> ElevationTypes { get; } = new List<ElevationType>();

        public ElevationType SelectedElevationType { get; set; }

        public DelegateCommand SelectPipeCommand { get; }
        public DelegateCommand SelectPointCommand { get; }
        public List<LabelStyle> PipeLabelStyles { get; set; } = new List<LabelStyle>();

        public LabelStyle SelectedPipeLabelStyle { get; set; }

        private string labelVariable;
        private Pipe pipe;

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

        public Point3d? Point { get; set; }

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
                SelectPipeCommand = new DelegateCommand(OnSelectPipeCommand);
                SelectPointCommand = new DelegateCommand(OnSelectPointCommand, OnSelectPointCommandCanExecute);
                EventAggregator = eventAggregator;
                LabelVariable = "$COORDINATE$";

                SelectLabelsToUpdateCommand = new DelegateCommand(OnSelectLabelsToUpdateCommand);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}", "Error");
            }
        }

        private bool OnSelectPointCommandCanExecute()
        {
            return Pipe != null;
        }

        private void OnSelectPointCommand()
        {
            try
            {
                if (Pipe == null)
                {
                    return;
                }

                Point = PromptUtils.PromptPoint("Pick point to set coordinate");

                if (SelectedPipeLabelStyle == null)
                {
                    return;
                }

                ObjectId labelStyleId = SelectedPipeLabelStyle.ObjectId;

                if (Pipe.GetPipeLabelIds().Contains(labelStyleId))
                {
                    MessageBox.Show("Label already exist", "Error");
                    return;
                }

                if (!Point.HasValue)
                {
                    return;
                }

                PipeArbitraryPoint pipeArbitraryPoint = PipeArbitraryPoint.Create(Pipe, new Point2d(Point.Value.X, Point.Value.Y));

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        Pipe pipeOpened = transaction.GetObject(Pipe.Id, OpenMode.ForWrite, false, true) as Pipe;
                        ObjectId labelId = PipeLabel.Create(pipeOpened.Id, pipeArbitraryPoint.RatioFromStartPoint, labelStyleId);
                        PipeLabel pipeLabel = transaction.GetObject(labelId, OpenMode.ForWrite, false, true) as PipeLabel;

                        ObjectIdCollection componentTextCollection = SelectedPipeLabelStyle.GetComponents(LabelStyleComponentType.Text);

                        pipeLabel.SetTextComponentOverride(transaction, LabelVariable, Math.Round(pipeArbitraryPoint.PipeCenterPoint.Z, 2).ToString());

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
