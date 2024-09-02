# Trimble EmulatorApplication

## Overview
The Trimble Application is a .NET MAUI-based mobile app designed for viewing and managing scenes and point cloud data. It provides functionality for creating new scenes, adding measurements, and visualizing point cloud data.

## Features

### Main Page
- Create new scenes
- View a list of existing scenes
- Navigate to the Point Cloud Viewer

### Scene Page
- View and edit scene details
- Add measurements to the scene (X and Y coordinates)
- Visualize measurements in a 2D graphical view
- Save scene data

### Point Cloud Viewer
- Load and view PLY files embedded in the application resources
- Visualize point cloud data in a 2D representation

## Usage

### Creating a New Scene
1. From the Main Page, tap "New Scene"
2. Enter a name for the scene
3. You will be taken to the Scene Page for the newly created scene

### Adding Measurements to a Scene
1. On the Scene Page, tap "Add Measurement"
2. Enter X and Y coordinates in the format "X,Y" (e.g., "10,20")
3. The new measurement will be added to the list and displayed in the graphical view

### Viewing Point Cloud Data
1. From the Main Page, tap "Open Point Cloud"
2. Select a PLY file from the list of embedded resources
3. The point cloud data will be displayed in a 2D representation

## Notes
- Due to time constraints, both the Point Cloud Viewer and Scene Detail pages currently only support 2D visualization. Future updates may include 3D visualization capabilities.
- The application uses embedded PLY files for point cloud data. Custom file loading is not currently supported.

## Technical Details
- Built using .NET MAUI for cross-platform compatibility
- Utilizes Microsoft.Maui.Graphics for rendering
- Implements custom drawables for point cloud and scene visualization

## Future Enhancements
- 3D visualization for point cloud data and scenes
- Custom file loading for PLY files
- Advanced measurement tools and analysis features

## Tested
- The application has been tested on local destop as well as pixel emulator

For any issues or feature requests, please contact the development team.