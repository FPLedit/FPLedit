﻿using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;

        public void Init(IInfo info)
        {
            this.info = info;

            info.Register<IExport>(new HtmlExport());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IBfplTemplate>(new Templates.BuchfahrplanTemplate());
            info.Register<IBfplTemplate>(new Templates.ZLBTemplate());
            info.Register<IFilterableUi>(new Forms.FilterableHandler());
            info.Register<IPreviewable>(new Forms.Preview());
            info.Register<IEditingDialog>(new Forms.VelocityDialogProxy());
        }
    }
}