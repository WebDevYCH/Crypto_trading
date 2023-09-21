﻿using Crypto.Core.Arbitrages.Dependency;
using Crypto.Core.Arbitrages.Deriatives;
using Crypto.Core.Common.Arbitrages;
using CryptoMarketClient.Common;
using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoMarketClient {
    public partial class DependencyArbitrageForm : XtraForm {
        public DependencyArbitrageForm() {
            InitializeComponent();
            // This line of code is generated by Data Source Configuration Wizard
            gridControl1.DataSource = new System.ComponentModel.BindingList<Crypto.Core.Common.Arbitrages.StatisticalArbitrageStrategy>() /* TODO: Assign the real data here.*/;
            FormBorderEffect = FormBorderEffect.None;
        }

        DependencyArbitrageHelper ArbitrageHelper { get; set; }

        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            InitializeArbitrageHelper();
            this.barCheckItem1.Checked = true;
        }

        void InitializeArbitrageHelper() {
            ArbitrageHelper = new DependencyArbitrageHelper("FuturesArbitrage");
            ArbitrageHelper.ItemChanged += OnArbitrageItemChanged;
            ArbitrageHelper.Load();
            this.gridControl1.DataSource = ArbitrageHelper.Items;
        }

        System.Windows.Forms.Timer saveTimer;
        protected System.Windows.Forms.Timer SaveTimer {
            get {
                if(saveTimer == null) {
                    saveTimer = new System.Windows.Forms.Timer();
                    saveTimer.Tick += OnSaveTimerTick;
                    saveTimer.Interval = 10 * 60 * 1000;
                }
                return saveTimer;
            }
        }

        private void OnSaveTimerTick(object sender, EventArgs e) {
            SaveHistory();
        }

        System.Windows.Forms.Timer updateTimer;
        protected System.Windows.Forms.Timer UpdateTimer {
            get {
                if(updateTimer == null) {
                    updateTimer = new System.Windows.Forms.Timer();
                    updateTimer.Tick += OnUpdateTimerTick;
                    updateTimer.Interval = 1000;
                }
                return updateTimer;
            }
        }

        protected void OnUpdateTimerTick(object sender, EventArgs e) {
            this.gridView1.RefreshData();
        }

        private void OnArbitrageItemChanged(object sender, DependencyArbitrageInfoChangedEventArgs e) {
            //BeginInvoke(new MethodInvoker(() => {
            //    if(!this.gridControl1.IsHandleCreated || this.gridControl1.IsDisposed || IsDisposed)
            //        return;
            //    this.gridView1.RefreshRow(this.gridView1.GetRowHandle(e.Arbitrage.Index));
            //} ));
        }

        private void biAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            //using(StatisticalArbitrageEditingForm form = new StatisticalArbitrageEditingForm()) {
            //    form.Add();
            //    if(form.ShowDialog() != DialogResult.OK)
            //        return;
            //    if(ArbitrageHelper.IsActive)
            //        form.Strategy.StartListenOrderBookStreams();
            //    ArbitrageHelper.Add(form.Strategy);
            //    ArbitrageHelper.Save();
            //    this.gridView1.RefreshData();
            //}
        }

        private void biEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            //using(StatisticalArbitrageEditingForm form = new StatisticalArbitrageEditingForm()) {
            //    form.Edit((StatisticalArbitrageStrategy)this.gridView1.GetFocusedRow());
            //    form.Strategy.StopListenOrderBookStreams();
            //    if(form.ShowDialog() != DialogResult.OK)
            //        return;
            //    ArbitrageHelper.Save();
            //    if(ArbitrageHelper.IsActive)
            //        form.Strategy.StartListenOrderBookStreams();
            //    this.gridView1.RefreshData();
            //}
        }

        private void biRemove_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            if(XtraMessageBox.Show("Dou you really want to remove selected items?", "Removing", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                return;
            List<StatisticalArbitrageStrategy> selectedItems = ArbitrageHelper.Items.Where(i => i.IsSelectedInDependencyArbitrageForm).ToList();
            foreach(StatisticalArbitrageStrategy item in selectedItems) {
                if(ArbitrageHelper.IsActive)
                    item.StartListenOrderBookStreams();
                ArbitrageHelper.Remove(item);
            }
            ArbitrageHelper.Save();
            this.gridView1.RefreshData();
        }

        private void biReconnect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            foreach(Exchange exchange in Exchange.Connected) {
                exchange.Reconnect();
            }
            ArbitrageHelper.StartWorking();
        }

        private void bbSaveHistory_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            SaveHistory();
        }
        void SaveHistory() {
            Thread t = new Thread(() => {
                for(int i = 0; i < ArbitrageHelper.Items.Count; i++) {
                    ArbitrageHelper.Items[i].SaveHistory();
                }
            });
            t.Start();
        }
        void LoadHistory() {
            for(int i = 0; i < ArbitrageHelper.Items.Count; i++) {
                ArbitrageHelper.Items[i].LoadHistory();
            }
        }

        private void biStart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            for(int i = 0; i < ArbitrageHelper.Items.Count; i++) {
                ArbitrageHelper.Items[i].History.Clear();
            }
            ArbitrageHelper.StartWorking();
            SaveTimer.Start();
            UpdateTimer.Start();
        }

        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e) {
            this.gridView1.CloseEditor();
        }

        private void biLoadHistory_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            LoadHistory();
        }

        private void biShowChart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            StatisticalArbitrageStrategy info = (StatisticalArbitrageStrategy)this.gridView1.GetFocusedRow();
            DependencyArbitrageChartForm form = info.Tag as DependencyArbitrageChartForm;
            if(form == null || form.IsDisposed) {
                form = new DependencyArbitrageChartForm();
                form.Text = "Current: " + info.TradingPair;
                form.Arbitrage = info;
            }
            form.Show();
            form.Activate();
        }

        private void bbExportHistory_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            XtraFolderBrowserDialog dialog = new XtraFolderBrowserDialog();
            if(dialog.ShowDialog() != DialogResult.OK)
                return;
            GridControl gc = new GridControl();
            GridView view = new GridView();
            gc.ViewCollection.Add(view);
            gc.MainView = view;
            gc.DataSource = new BindingSource() { DataSource = typeof(DependencyArbitrageHistoryItem) };
            gc.BindingContext = new BindingContext();
            gc.ForceInitialize();
            view.PopulateColumns();
            ArbitrageHelper.Items.ForEach(i => i.SaveHistory());
            ArbitrageHelper.Items.ForEach(i => {
                StatisticalArbitrageStrategy info = new StatisticalArbitrageStrategy();
                info.Assign(i);
                info.LoadHistory();
                gc.DataSource = info.History;
                gc.ExportToXlsx(dialog.SelectedPath + "\\" + info.GetExportFileName() + ".xlsx");
            });
            System.Diagnostics.Process.Start(dialog.SelectedPath);
        }

        private void biAllHistoryChart_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            StatisticalArbitrageStrategy info = (StatisticalArbitrageStrategy)this.gridView1.GetFocusedRow();
            DependencyArbitrageChartForm form = new DependencyArbitrageChartForm();

            StatisticalArbitrageStrategy all = new StatisticalArbitrageStrategy();
            all.Assign(info);
            all.LoadHistory();

            form.Text = "All History: " + info.TradingPair;
            form.Arbitrage = all;

            form.Show();
            form.Activate();
        }

        private void barCheckItem1_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            this.dpLog.Visibility = this.barCheckItem1.Checked ? DevExpress.XtraBars.Docking.DockVisibility.Visible : DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            if(!this.barCheckItem1.Checked)
                this.gcLog.DataSource = null;
            else
                this.gcLog.DataSource = new RealTimeSource() { DataSource = LogManager.Default.Messages };
        }
    }
}
