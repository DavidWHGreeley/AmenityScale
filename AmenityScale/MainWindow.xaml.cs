/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2015-24-01  Greeley         Rewrite from RESTful to WPF


using AmenityScaleCore.Data;
using AmenityScaleCore.Models.Amenity;
using AmenityScaleCore.Models.Location;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AmenityScale
{
    public partial class MainWindow
    {
        private readonly AmenityDataAccess _amenityRepo = new AmenityDataAccess();
        private readonly LocationDataAccess _locationRepo = new LocationDataAccess();

        private readonly ObservableCollection<AmenityDTO> _amenities = new ObservableCollection<AmenityDTO>();
        private readonly ObservableCollection<LocationDTO> _locations = new ObservableCollection<LocationDTO>();

        private AmenityDTO _selectedAmenity = null;
        private LocationDTO _selectedLocation = null;

        private enum DrawerMode
        {
            None,
            AmenityCreate,
            AmenityUpdate,
            LocationCreate,
            LocationUpdate
        }

        private DrawerMode _drawerMode = DrawerMode.None;

        public MainWindow()
        {
            InitializeComponent();

            AmenitiesGrid.ItemsSource = _amenities;
            LocationsGrid.ItemsSource = _locations;

            Loaded += (s, e) =>
            {
                TryLoadLookups();
                RefreshAmenities();
                RefreshLocations();
                CloseDrawer(false);
            };
        }

        private void TryLoadLookups()
        {
            try
            {
                var categories = _amenityRepo.ReadCategories();

                // TODO: Put Subdivision in a shard file (Maybe the lookup.cs)
                var amenitySubdivisions = _amenityRepo.ReadSubdivisions();
                var locationSubdivisions = _locationRepo.ReadSubdivisions();

                CategoryCombo.ItemsSource = categories;
                CategoryCombo.DisplayMemberPath = "CategoryName";
                CategoryCombo.SelectedValuePath = "CategoryID";

                SubdivisionCombo.ItemsSource = amenitySubdivisions;
                SubdivisionCombo.DisplayMemberPath = "SubdivisionName";
                SubdivisionCombo.SelectedValuePath = "SubdivisionID";

                LocationSubdivisionCombo.ItemsSource = locationSubdivisions;
                LocationSubdivisionCombo.DisplayMemberPath = "SubdivisionName";
                LocationSubdivisionCombo.SelectedValuePath = "SubdivisionID";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lookup load failed");
            }
        }


        private void RefreshAmenities()
        {
            try
            {
                var rows = _amenityRepo.ReadAll();
                _amenities.Clear();
                for (int i = 0; i < rows.Count; i++)
                    _amenities.Add(rows[i]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Amenity Read failed:\n" + ex.Message);
            }
        }

        private void AmenitiesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedAmenity = AmenitiesGrid.SelectedItem as AmenityDTO;
        }

        private void AmenityRead_Click(object sender, RoutedEventArgs e) => RefreshAmenities();

        private void AmenityCreate_Click(object sender, RoutedEventArgs e)
        {
            _drawerMode = DrawerMode.AmenityCreate;
            DrawerTitle.Text = "Create Amenity";
            ShowAmenityDrawer();
            ClearAmenityForm();
            OpenDrawer();
        }

        private void AmenityUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAmenity == null)
            {
                MessageBox.Show("Select an amenity first.");
                return;
            }

            _drawerMode = DrawerMode.AmenityUpdate;
            DrawerTitle.Text = "Update Amenity";
            ShowAmenityDrawer();
            FillAmenityForm(_selectedAmenity);
            OpenDrawer();
        }

        private void AmenityDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAmenity == null)
            {
                MessageBox.Show("Select an amenity first.");
                return;
            }

            var ok = MessageBox.Show("Delete selected amenity?", "Confirm", MessageBoxButton.YesNo);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                _amenityRepo.Delete(_selectedAmenity.AmenityID);
                RefreshAmenities();
                _selectedAmenity = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Amenity Delete failed:\n" + ex.Message);
            }
        }

        private void ClearAmenityForm()
        {
            AmenityNameInput.Text = "";
            StreetInput.Text = "";
            CityInput.Text = "";

            CategoryCombo.SelectedIndex = -1;
            SubdivisionCombo.SelectedIndex = -1;

            GeoPointRadio.IsChecked = true;
            LatitudeInput.Text = "";
            LongitudeInput.Text = "";
            LocationWktInput.Text = "";

            UpdateGeoPanels();
        }

        private void FillAmenityForm(AmenityDTO d)
        {
            AmenityNameInput.Text = d.Name ?? "";
            StreetInput.Text = d.Street ?? "";
            CityInput.Text = d.City ?? "";

            try
            {
                CategoryCombo.SelectedValue = d.CategoryID;
                SubdivisionCombo.SelectedValue = d.SubdivisionID;
            }
            catch { }

            if (string.Equals(d.GeometryType, "WKT", StringComparison.OrdinalIgnoreCase))
            {
                GeoWktRadio.IsChecked = true;
                LocationWktInput.Text = d.LocationWKT ?? "";
            }
            else
            {
                GeoPointRadio.IsChecked = true;
                LatitudeInput.Text = d.Latitude.HasValue
                    ? d.Latitude.Value.ToString(CultureInfo.InvariantCulture)
                    : "";
                LongitudeInput.Text = d.Longitude.HasValue
                    ? d.Longitude.Value.ToString(CultureInfo.InvariantCulture)
                    : "";
            }

            UpdateGeoPanels();
        }

        private AmenityDTO ReadAmenityForm(int amenityId)
        {
            if (string.IsNullOrWhiteSpace(AmenityNameInput.Text))
                throw new Exception("Amenity name is required.");

            int categoryId = CategoryCombo.SelectedValue == null ? 0 : (int)CategoryCombo.SelectedValue;
            int subdivisionId = SubdivisionCombo.SelectedValue == null ? 0 : (int)SubdivisionCombo.SelectedValue;

            bool isWkt = GeoWktRadio.IsChecked == true;

            decimal lat = 0, lng = 0;
            string wkt = null;

            if (isWkt)
            {
                wkt = (LocationWktInput.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(wkt))
                    throw new Exception("WKT is required when Geometry Type is WKT.");
            }
            else
            {
                if (!decimal.TryParse(LatitudeInput.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lat))
                    throw new Exception("Latitude must be a number.");
                if (!decimal.TryParse(LongitudeInput.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lng))
                    throw new Exception("Longitude must be a number.");
            }

            return new AmenityDTO
            {
                AmenityID = amenityId,
                Name = AmenityNameInput.Text.Trim(),
                Street = StreetInput.Text.Trim(),
                City = CityInput.Text.Trim(),
                CategoryID = categoryId,
                SubdivisionID = subdivisionId,
                Latitude = lat,
                Longitude = lng,
                GeometryType = isWkt ? "WKT" : "Point",
                LocationWKT = wkt
            };
        }

        private void RefreshLocations()
        {
            try
            {
                var rows = _locationRepo.ReadAll();
                _locations.Clear();
                for (int i = 0; i < rows.Count; i++)
                    _locations.Add(rows[i]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Location Read failed:\n" + ex.Message);
            }
        }

        private void LocationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedLocation = LocationsGrid.SelectedItem as LocationDTO;
        }

        private void LocationRead_Click(object sender, RoutedEventArgs e) => RefreshLocations();

        private void LocationCreate_Click(object sender, RoutedEventArgs e)
        {
            _drawerMode = DrawerMode.LocationCreate;
            DrawerTitle.Text = "Create Location";
            ShowLocationDrawer();
            ClearLocationForm();
            OpenDrawer();
        }

        private void LocationUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLocation == null)
            {
                MessageBox.Show("Select a location first.");
                return;
            }

            _drawerMode = DrawerMode.LocationUpdate;
            DrawerTitle.Text = "Update Location";
            ShowLocationDrawer();
            FillLocationForm(_selectedLocation);
            OpenDrawer();
        }

        private void LocationDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLocation == null)
            {
                MessageBox.Show("Select a location first.");
                return;
            }

            var ok = MessageBox.Show("Delete selected location?", "Confirm", MessageBoxButton.YesNo);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                _locationRepo.Delete(_selectedLocation.LocationID);
                RefreshLocations();
                _selectedLocation = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Location Delete failed:\n" + ex.Message);
            }
        }

        private void ClearLocationForm()
        {
            LocationNameInput.Text = "";
            StreetNumberInput.Text = "";
            LocationStreetInput.Text = "";
            LocationCityInput.Text = "";
            LocationLatitudeInput.Text = "";
            LocationLongitudeInput.Text = "";
            LocationSubdivisionCombo.SelectedIndex = -1;
        }

        private void FillLocationForm(LocationDTO d)
        {
            LocationNameInput.Text = d.LocationName ?? "";
            StreetNumberInput.Text = d.StreetNumber ?? "";
            LocationStreetInput.Text = d.Street ?? "";
            LocationCityInput.Text = d.City ?? "";

            try { LocationSubdivisionCombo.SelectedValue = d.SubdivisionID; } catch { }

            if (string.Equals(d.GeometryType, "WKT", StringComparison.OrdinalIgnoreCase))
            {
                GeoWktRadio.IsChecked = true;
                LocationWktInput.Text = d.LocationWKT ?? "";
            }
            else
            {
                GeoPointRadio.IsChecked = true;
                LatitudeInput.Text = d.Latitude.HasValue
                    ? d.Latitude.Value.ToString(CultureInfo.InvariantCulture)
                    : "";
                LongitudeInput.Text = d.Longitude.HasValue
                    ? d.Longitude.Value.ToString(CultureInfo.InvariantCulture)
                    : "";
            }
            UpdateGeoPanels();
        }

        private LocationDTO ReadLocationForm(int locationId)
        {
            if (LocationSubdivisionCombo.SelectedValue == null)
                throw new Exception("Subdivision is required.");

            decimal lat, lng;
            if (!decimal.TryParse(LocationLatitudeInput.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lat))
                throw new Exception("Latitude must be a number.");
            if (!decimal.TryParse(LocationLongitudeInput.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lng))
                throw new Exception("Longitude must be a number.");

            return new LocationDTO
            {
                LocationID = locationId,
                LocationName = LocationNameInput.Text.Trim(),
                StreetNumber = StreetNumberInput.Text.Trim(),
                Street = LocationStreetInput.Text.Trim(),
                City = LocationCityInput.Text.Trim(),
                SubdivisionID = (int)LocationSubdivisionCombo.SelectedValue,
                Latitude = lat,
                Longitude = lng
            };
        }

        private void ShowAmenityDrawer()
        {
            AmenityFormPanel.Visibility = Visibility.Visible;
            LocationFormPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowLocationDrawer()
        {
            AmenityFormPanel.Visibility = Visibility.Collapsed;
            LocationFormPanel.Visibility = Visibility.Visible;
        }

        private void OpenDrawer()
        {
            Overlay.Visibility = Visibility.Visible;

            var anim = new DoubleAnimation
            {
                From = DrawerTranslate.X,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DrawerTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
        }

        private void CloseDrawer(bool animate)
        {
            Overlay.Visibility = Visibility.Collapsed;

            if (!animate)
            {
                DrawerTranslate.X = Drawer.ActualWidth > 0 ? Drawer.ActualWidth : 380;
                return;
            }

            var toX = Drawer.ActualWidth > 0 ? Drawer.ActualWidth : 380;

            var anim = new DoubleAnimation
            {
                From = DrawerTranslate.X,
                To = toX,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            DrawerTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
        }

        private void CloseDrawer_Click(object sender, RoutedEventArgs e) { CloseDrawer(true); }

        private void Overlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { CloseDrawer(true); }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_drawerMode == DrawerMode.AmenityCreate)
                {
                    var dto = ReadAmenityForm(0);
                    _amenityRepo.Create(dto);
                    RefreshAmenities();
                    CloseDrawer(true);
                    return;
                }

                if (_drawerMode == DrawerMode.AmenityUpdate)
                {
                    if (_selectedAmenity == null) throw new Exception("No selected amenity.");
                    var dto = ReadAmenityForm(_selectedAmenity.AmenityID);
                    _amenityRepo.Update(dto);
                    RefreshAmenities();
                    CloseDrawer(true);
                    return;
                }

                if (_drawerMode == DrawerMode.LocationCreate)
                {
                    var dto = ReadLocationForm(0);
                    _locationRepo.Create(dto);
                    RefreshLocations();
                    CloseDrawer(true);
                    return;
                }

                if (_drawerMode == DrawerMode.LocationUpdate)
                {
                    if (_selectedLocation == null) throw new Exception("No selected location.");
                    var dto = ReadLocationForm(_selectedLocation.LocationID);
                    _locationRepo.Update(dto);
                    RefreshLocations();
                    CloseDrawer(true);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed:\n" + ex.Message);
            }
        }


        private void GeoType_Checked(object sender, RoutedEventArgs e) => UpdateGeoPanels();

        private void UpdateGeoPanels()
        {
            if (GeoWktRadio == null || LatLngPanel == null || WktPanel == null)
                return;

            bool isWkt = GeoWktRadio.IsChecked == true;

            LatLngPanel.Visibility = isWkt ? Visibility.Collapsed : Visibility.Visible;
            WktPanel.Visibility = isWkt ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}


