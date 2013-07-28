﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TAlex.BeautifulFractals.Fractals;
using TAlex.BeautifulFractals.Rendering;
using TAlex.BeautifulFractals.Services;
using TAlex.BeautifulFractals.Services.Licensing;
using TAlex.BeautifulFractals.Services.Windows;
using TAlex.Common.Environment;
using TAlex.Common.Licensing;
using TAlex.WPF.Mvvm;
using TAlex.WPF.Mvvm.Commands;


namespace TAlex.BeautifulFractals.ViewModels
{
    public class PreferencesWindowViewModel : ViewModelBase
    {
        #region Fields

        protected readonly IAppSettings AppSettings;
        protected readonly ApplicationInfo ApplicationInfo;
        protected readonly LicenseBase AppLicense;
        protected readonly IFractalsManager FractalManager;
        protected readonly IFontChooserDialogService FontChooserDialogService;
        protected readonly BeautifulFractals.Infrastructure.ICollectionViewFactory CollectionViewFactory;

        private ObservableCollection<Fractal> _fractals;
        public BeautifulFractals.Infrastructure.ICollectionView _fractalsView;
        private string _fractalsSearchQuery;
        private bool _closeSignal;

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                string baseTitle = String.Format("{0} Preferences", ApplicationInfo.Title);

                if (AppLicense.IsTrial)
                    return String.Format("{0} (days left: {1})", baseTitle, AppLicense.TrialDaysLeft);
                else
                    return baseTitle;
            }
        }


        public Color PrimaryBackColor
        {
            get
            {
                return AppSettings.PrimaryBackColor;
            }

            set
            {
                AppSettings.PrimaryBackColor = value;
                RaisePropertyChanged(() => PrimaryBackColor);
            }
        }

        public Color SecondaryBackColor
        {
            get
            {
                return AppSettings.SecondaryBackColor;
            }

            set
            {
                AppSettings.SecondaryBackColor = value;
                RaisePropertyChanged(() => SecondaryBackColor);
            }
        }

        public BackgroundGradientType BackGradientType
        {
            get
            {
                return AppSettings.BackGradientType;
            }

            set
            {
                AppSettings.BackGradientType = value;
                RaisePropertyChanged(() => BackGradientType);
            }
        }

        public TimeSpan Delay
        {
            get
            {
                return AppSettings.Delay;
            }

            set
            {
                AppSettings.Delay = value;
                RaisePropertyChanged(() => Delay);
            }
        }

        public bool RandomOrder
        {
            get
            {
                return AppSettings.RandomOrder;
            }

            set
            {
                AppSettings.RandomOrder = value;
                RaisePropertyChanged(() => RandomOrder);
            }
        }

        public bool ExitOnMouseMove
        {
            get
            {
                return AppSettings.ExitOnMouseMove;
            }

            set
            {
                AppSettings.ExitOnMouseMove = value;
                RaisePropertyChanged(() => ExitOnMouseMove);
            }
        }

        public bool ShowFractalCaptions
        {
            get
            {
                return AppSettings.ShowFractalCaptions;
            }

            set
            {
                AppSettings.ShowFractalCaptions = value;
                RaisePropertyChanged(() => ShowFractalCaptions);
            }
        }


        public string CaptionFontFamily
        {
            get
            {
                return AppSettings.CaptionFontFamily;
            }

            set
            {
                AppSettings.CaptionFontFamily = value;
            }
        }

        public double CaptionFontSize
        {
            get
            {
                return AppSettings.CaptionFontSize;
            }

            set
            {
                AppSettings.CaptionFontSize = value;
            }
        }

        public Rendering.Color CaptionFontColor
        {
            get
            {
                return AppSettings.CaptionFontColor; 
            }

            set
            {
                AppSettings.CaptionFontColor = value;
            }
        }

        public string FractalsSearchQuery
        {
            get
            {
                return _fractalsSearchQuery;
            }

            set
            {
                Set(() => FractalsSearchQuery, ref _fractalsSearchQuery, value);
                _fractalsView.Refresh();
            }
        }

        public ObservableCollection<Fractal> Fractals
        {
            get
            {
                return _fractals;
            }

            set
            {
                _fractals = value;
                _fractalsView = CollectionViewFactory.GetView(_fractals);
                _fractalsView.Filter = FractalFilter;
            }
        }

        public BeautifulFractals.Infrastructure.ICollectionView FractalsView
        {
            get
            {
                return _fractalsView;
            }
        }

        public bool CloseSignal
        {
            get
            {
                return _closeSignal;
            }

            set
            {
                Set(() => CloseSignal, ref _closeSignal, value);
            }
        }

        #endregion

        #region Commands

        public ICommand SwapBackgroundColorsCommand { get; set; }

        public ICommand OpenCaptionStyleChooserDialogCommand { get; set; }

        public ICommand ClearSearchQueryCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        #endregion

        #region Constructors

        public PreferencesWindowViewModel(IAppSettings appSettings, ApplicationInfo applicationInfo, LicenseBase appLicense, FontChooserDialogService fontChooserDialogService, IFractalsManager fractalManager, BeautifulFractals.Infrastructure.ICollectionViewFactory collectionViewFactory)
        {
            AppSettings = appSettings;
            ApplicationInfo = applicationInfo;
            AppLicense = appLicense;
            FontChooserDialogService = fontChooserDialogService;
            FractalManager = fractalManager;
            CollectionViewFactory = collectionViewFactory;

            LoadFractals();
            InitCommands();
        }

        #endregion

        #region Methods

        private void InitCommands()
        {
            SwapBackgroundColorsCommand = new RelayCommand(SwapBackgroundColorsCommandExecute);
            OpenCaptionStyleChooserDialogCommand = new RelayCommand(OpenCaptionStyleChooserDialogCommandExecute);
            ClearSearchQueryCommand = new RelayCommand(ClearSearchQueryExecute, ClearSearchQueryCanExecute);
            SaveCommand = new RelayCommand(SaveCommandExecute);
            CancelCommand = new RelayCommand(CancelCommandExecute);
        }

        private void LoadFractals()
        {
            Fractals = FractalManager.Load(AppSettings.FractalsCollectionPath);
        }


        private void SwapBackgroundColorsCommandExecute()
        {
            var temp = PrimaryBackColor;
            PrimaryBackColor = SecondaryBackColor;
            SecondaryBackColor = temp;
        }

        private void OpenCaptionStyleChooserDialogCommandExecute()
        {
            FontChooserDialogService.SelectedFontFamily = CaptionFontFamily;
            FontChooserDialogService.SelectedFontSize = CaptionFontSize;
            FontChooserDialogService.SelectedFontColor = CaptionFontColor;

            if (FontChooserDialogService.ShowDialog() == true)
            {
                CaptionFontFamily = FontChooserDialogService.SelectedFontFamily;
                CaptionFontSize = FontChooserDialogService.SelectedFontSize;
                CaptionFontColor = FontChooserDialogService.SelectedFontColor;
            }
        }

        private bool ClearSearchQueryCanExecute()
        {
            return !String.IsNullOrEmpty(FractalsSearchQuery);
        }

        private void ClearSearchQueryExecute()
        {
            FractalsSearchQuery = String.Empty;
        }

        private void SaveCommandExecute()
        {
            AppSettings.Save();
            CloseSignal = true;
        }

        public void CancelCommandExecute()
        {
            CloseSignal = true;
        }


        private bool FractalFilter(object o)
        {
            Fractal fractal = o as Fractal;

            var query = (FractalsSearchQuery + String.Empty).ToLower();
            if (String.IsNullOrWhiteSpace(query))
            {
                return true;
            }
            string[] captionParts = fractal.Caption.ToLower().Split(' ');

            foreach (var captionPart in captionParts)
            {
                if (captionPart.StartsWith(query))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }

    public enum BackgroundGradientType
    {
        Vertical,
        Horizontal,
        ForwardDiagonal,
        BackwardDiagonal
    }
}
