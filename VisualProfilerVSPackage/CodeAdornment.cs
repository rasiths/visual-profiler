using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using VisualProfilerVSPackage.View;
using ContainingUnitView = VisualProfilerVSPackage.View.ContainingUnitView;
using System.Linq;

namespace CodeAdornment
{
    /// <summary>
    /// Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    class CodeAdornment
    {

        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;
        private string _sourceFilePath;

        public CodeAdornment(IWpfTextView view)
        {
            _view = view;
            _sourceFilePath = GetSourceFilePath();
            _adornmentLayer = view.GetAdornmentLayer("CodeAdornment");
            _view.ViewportWidthChanged += delegate { Initialize(); };
            _view.ViewportHeightChanged += delegate { Initialize(); };
            //  _view.LayoutChanged += delegate { Initialize(); }; 
        }

        private bool _init = false;
        private void Initialize()
        {
            if (_init) return;
            _init = true;

            
            _adornmentLayer.RemoveAllAdornments();

            SourceLineHeightConverter.LineHeight = _view.LineHeight -1 ;
            ContainingUnitView containingUnitView = ContainingUnitView.GetContainingUnitViewByName(_sourceFilePath);


            if (containingUnitView.Parent != null)
            {
                var adornmentLayer = (IAdornmentLayer)containingUnitView.Parent;
                adornmentLayer.RemoveAdornment(containingUnitView);
            }


            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.OwnerControlled, null, null, containingUnitView, null);
        }

        private string GetSourceFilePath()
        {
            Microsoft.VisualStudio.Text.ITextDocument document;
            if ((_view == null) ||
                    (!_view.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(Microsoft.VisualStudio.Text.ITextDocument), out document)))
                return String.Empty;

            // If we have no document, just ignore it.
            if ((document == null) || (document.TextBuffer == null))
                return String.Empty;

            return document.FilePath;
        }
    }
}
