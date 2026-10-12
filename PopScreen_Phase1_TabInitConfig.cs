// ===== Pop Screen.cs에 추가할 코드 (Phase 1) =====

// 1. PopScreen 클래스 내에 추가할 필드 (line 99 이후)
        /// <summary>
        /// Phase 1: Tab initialization configuration stored as data structure.
        /// Each tab's initialization info is managed in one place; adding new tabs only requires adding to this array.
        /// </summary>
        private TabInitConfig[] _tabConfigs;

// ===== PopScreen 클래스 외부에 추가할 코드 (enum PopTab 이전) =====

    /// <summary>
    /// Phase 1: Encaps ulates tab initialization configuration.
    /// Used to centralize tab setup logic and enable data-driven tab management.
    /// </summary>
    internal class TabInitConfig
    {
        public PopTab TabType { get; set; }
        public DevExpress.XtraGrid.GridControl GridControl { get; set; }
        public SEOJIN_TMES.Client.Forms.Common.MeisGridView GridView { get; set; }
        public System.Drawing.Color ThemeColor { get; set; }
        public string TabName { get; set; }
        public System.Func<System.Threading.Tasks.Task> LoadDataAsync { get; set; }
        public System.Action ConfigureColumns { get; set; }
    }

// ===== PopScreen 클래스에 추가할 메서드 =====

        /// <summary>
        /// Phase 1: Initialize tab configuration array.
        /// This method is called from PopScreen_Load() to set up all tab configs.
        /// </summary>
        private void InitializeTabConfigs()
        {
            _tabConfigs = new TabInitConfig[]
            {
                new TabInitConfig
                {
                    TabType = PopTab.Material,
                    GridControl = gridMaterial,
                    GridView = gridViewMaterial,
                    ThemeColor = AppTheme.Emerald,
                    TabName = "Material",
                    LoadDataAsync = FetchAndBindMaterialAsync,
                    ConfigureColumns = ConfigureMaterialGrid
                },
                new TabInitConfig
                {
                    TabType = PopTab.BomSpec,
                    GridControl = gridBomSpec,
                    GridView = gridViewBomSpec,
                    ThemeColor = AppTheme.Indigo,
                    TabName = "BOM",
                    LoadDataAsync = FetchAndBindBomAsync,
                    ConfigureColumns = ConfigureBomSpecGrid
                },
                new TabInitConfig
                {
                    TabType = PopTab.Inspection,
                    GridControl = gridInspection,
                    GridView = gridViewInspection,
                    ThemeColor = AppTheme.Violet,
                    TabName = "Inspection",
                    LoadDataAsync = FetchAndBindInspectionAsync,
                    ConfigureColumns = ConfigureInspectionGrid
                },
                new TabInitConfig
                {
                    TabType = PopTab.Equipment,
                    GridControl = gridEquipment,
                    GridView = gridViewEquipment,
                    ThemeColor = AppTheme.Cyan,
                    TabName = "Equipment",
                    LoadDataAsync = FetchAndBindEquipmentAsync,
                    ConfigureColumns = ConfigureEquipmentGrid
                }
            };
        }

        /// <summary>
        /// Phase 1: Unified tab initialization method.
        /// All tabs are initialized using the same pattern from _tabConfigs array.
        /// </summary>
        private async Task InitializeAllTabsAsync()
        {
            foreach (var config in _tabConfigs)
            {
                try
                {
                    // Apply theme to grid
                    AppTheme.Grid(config.GridControl, config.GridView, config.ThemeColor);

                    // Configure grid columns
                    config.ConfigureColumns();

                    // Load data
                    await config.LoadDataAsync();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Tab initialization failed: {config.TabName}", ex);
                    AppMessageBox.Warn(this, 
                        string.Format("Tab '{0}' initialization error: {1}", config.TabName, ex.Message), 
                        Language.Pop_Title);
                }
            }
        }

        /// <summary>
        /// Phase 2: Generic data loading and binding method.
        /// Replaces all LoadXxxAsync() methods with unified error handling and logging.
        /// </summary>
        private async Task<List<T>> LoadAndBindGridAsync<T>(
            DevExpress.XtraGrid.GridControl grid,
            System.Func<System.Threading.Tasks.Task<List<T>>> fetcher,
            string tabName) where T : class
        {
            try
            {
                var data = await fetcher();
                if (data == null)
                    data = new List<T>();

                grid.DataSource = data;
                Logger.Info($"Tab '{tabName}' data loaded successfully: {data.Count} items");

                return data;
            }
            catch (ApiException ex)
            {
                Logger.Error($"Tab '{tabName}' data loading failed: {ex.Message}", ex);
                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return new List<T>();
            }
            catch (Exception ex)
            {
                Logger.Error($"Tab '{tabName}' unexpected error", ex);
                AppMessageBox.Fatal(this, ex.Message, Language.Pop_Title);
                return new List<T>();
            }
        }

        // ===== Phase 2: Tab-specific async wrappers for data loading =====

        /// <summary>
        /// Phase 2: Material tab data fetch and bind.
        /// Wraps the generic LoadAndBindGridAsync with Material-specific logic.
        /// </summary>
        private async Task FetchAndBindMaterialAsync()
        {
            _materials = await LoadAndBindGridAsync(
                gridMaterial,
                FetchMaterialsAsync,
                "Material"
            );
        }

        /// <summary>
        /// Phase 2: BOM tab data fetch and bind with additional highlighting logic.
        /// </summary>
        private async Task FetchAndBindBomAsync()
        {
            _bomSpecs = await LoadAndBindGridAsync(
                gridBomSpec,
                FetchBomSpecsAsync,
                "BOM"
            );

            // Apply BOM-specific highlighting for torque results
            ApplyBomSpecHighlighting(_bomSpecs);
        }

        /// <summary>
        /// Phase 2: Inspection tab data fetch and bind.
        /// </summary>
        private async Task FetchAndBindInspectionAsync()
        {
            _inspections = await LoadAndBindGridAsync(
                gridInspection,
                FetchInspectionsAsync,
                "Inspection"
            );
        }

        /// <summary>
        /// Phase 2: Equipment tab data fetch and bind with equipment state refresh.
        /// </summary>
        private async Task FetchAndBindEquipmentAsync()
        {
            _equipments = await LoadAndBindGridAsync(
                gridEquipment,
                FetchEquipmentsAsync,
                "Equipment"
            );

            // Refresh equipment connection states
            RefreshEquipmentStates();
        }

        /// <summary>
        /// Helper method for BOM tab: Highlight torque results outside specification range.
        /// Extracted from original LoadBomAsync() method.
        /// </summary>
        private void ApplyBomSpecHighlighting(List<PopBomSpecDto> specs)
        {
            if (specs == null || specs.Count == 0)
                return;

            for (int i = 0; i < gridViewBomSpec.RowCount; i++)
            {
                var target = gridViewBomSpec.GetRow(i) as PopBomSpecDto;
                if (target == null)
                    continue;

                if (decimal.TryParse(target.TorqueResult?.ToString(), out decimal value))
                {
                    decimal.TryParse(target.TorqueLower?.ToString(), out decimal min);
                    decimal.TryParse(target.TorqueUpper?.ToString(), out decimal max);

                    if (value < min || value > max)
                    {
                        gridViewBomSpec.Columns["TorqueResult"].AppearanceCell.ForeColor = AppTheme.Rose;
                    }
                    else
                    {
                        gridViewBomSpec.FocusedRowHandle = i;
                    }
                }
            }
        }
