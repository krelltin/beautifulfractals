﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TAlex.BeautifulFractals.Fractals;
using TAlex.BeautifulFractals.Infrastructure;
using TAlex.BeautifulFractals.Rendering;
using TAlex.WPF.Mvvm;
using TAlex.WPF.Mvvm.Commands;


namespace TAlex.BeautifulFractals.ViewModels
{
    public class PreviewWindowViewModel : ViewModelBase
    {
        #region Fields

        private static readonly int ResizeTimerInterval = 2000;

        private string _title;
        private bool _isBusy;
        private ImageSource _plot;

        private double _currPlotWidth;
        private double _currPlotHeight;
        private Fractal _currFractal;
        private Timer _refreshTimer;

        protected ICollectionView FractalCollection { get; set; }

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                Set(() => Title, ref _title, value);
            }
        }

        public double PlotWidth { get; set; }

        public double PlotHeight { get; set; }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                Set(() => IsBusy, ref _isBusy, value);
            }
        }

        public ImageSource Plot
        {
            get
            {
                return _plot;
            }

            set
            {
                Set(() => Plot, ref _plot, value);
            }
        }

        public ICommand ShowPrevCommand { get; set; }

        public ICommand ShowNextCommand { get; set; }

        #endregion

        #region Constructors

        public PreviewWindowViewModel(ICollectionView fractalCollection)
        {
            RegisterCommands();
            FractalCollection = fractalCollection;

            _refreshTimer = new Timer(ResizeCallback, null, 100, ResizeTimerInterval);
        }

        #endregion

        #region Methods

        public void RenderFractal()
        {
            if (IsBusy) return;
            IsBusy = true;

            Task.Run(() =>
            {
                WriteableBitmap wb = BitmapFactory.New((int)PlotWidth, (int)PlotHeight);
                Fractal2D targetFractal = FractalCollection.CurrentItem as Fractal2D;

                _currPlotWidth = PlotWidth;
                _currPlotHeight = PlotHeight;
                _currFractal = targetFractal;

                if (targetFractal != null)
                {
                    IGraphics2DContext context = new WriteableBitmapGraphicsContext(wb);
                    targetFractal.Render(context);
                    context.Invalidate();

                    Title = String.Format(Properties.Resources.locPreviewWindowTitle, targetFractal.Caption);
                }
                
                Plot = wb;
                IsBusy = false;
            });
        }

        private void RegisterCommands()
        {
            ShowPrevCommand = new RelayCommand(ShowPrevCommandExecute, ShowPrevCommandCanExecute);
            ShowNextCommand = new RelayCommand(ShowNextCommandExecute, ShowNextCommandCanExecute);
        }

        private void ShowPrevCommandExecute()
        {
            FractalCollection.MoveCurrentToPrevious();
            RenderFractal();
        }

        private bool ShowPrevCommandCanExecute()
        {
            return FractalCollection.CurrentPosition > 0;
        }

        private void ShowNextCommandExecute()
        {
            FractalCollection.MoveCurrentToNext();
            RenderFractal();
        }

        private bool ShowNextCommandCanExecute()
        {
            return FractalCollection.CurrentPosition < FractalCollection.OfType<Fractal>().Count() - 1;
        }


        private void ResizeCallback(object state)
        {
            if (_currPlotWidth != PlotWidth || _currPlotHeight != PlotHeight || _currFractal != FractalCollection.CurrentItem)
            {
                RenderFractal();
            }
        }

        #endregion
    }
}