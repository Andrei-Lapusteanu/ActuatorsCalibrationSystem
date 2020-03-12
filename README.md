# ActuatorsCalibrationSystem

## Summary

 Windows application which using image processing algorithms can determine the movement behaviour of a set of mechanical actuators. This application represents my bachelor's project
 
### Scope
- This application is part of the **ELI-NP (Extreme Light Infrastructure-Nuclear Physics)** project - [http://www.eli-np.ro/ro/](http://www.eli-np.ro/ro/)
- The software, using the video feed captured from a highly specialized capturing device, has to determine in physical space the point of incidence of a high energy beam fired inside an interaction chamber. The beam is required to interact with a specialized probe whose position can be controlled using a 3-axis stepper motor system. Thus, using the image captured from the camera, the software aims to reposition the probe inside the interaction chamber with the help of the mechanical actuator (stepper motor) system

<a href="https://imgur.com/VNuFmMj"><img src="https://i.imgur.com/VNuFmMj.png" title="source: imgur.com" /></a>
 
## Technologies

- Mostly written using **C#**, using the **.NET framework**
- UI was designed in **WPF/XAML**
- Its backbone is a modified **MVC (Model-View-Controller)** design pattern, to which an **Entites** project was added
- **OpenCVSharp** was used in order to implement the image processing requirements
- Basic logging was implemented using **log4net**

## Functionality  - software integration
-image-
### Important
Currently, because the software requires the capturing device and actuators connected as peripherals, demonstration of its functionality cannot be fully achieved. There exists a possibility of virtualizing these peripherals, which I will tackle in the near fututre, in order to properly build the solution
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTAyNjUxMjE5MCwxMTM4NzM4MDEwLC03Mz
M0MDI2OTIsLTIxMzgwNzIzNDEsMzc3MjU0MTUxLC01ODU2MzIw
MjEsMTY3OTE4OTUxM119
-->