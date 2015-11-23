using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

//Estas referencias son necesarias para usar GLIDE
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;

namespace Practica4DSCC
{
    public partial class Program
    {
        //Objetos de interface gráfica GLIDE
        private GHI.Glide.Display.Window iniciarWindow, temperatureWindow;
        private Button btn_inicio;
        private GT.Timer timer;
        private TextBlock text,tmp;
        private static String temperature;
        private ProgressBar progressBar;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");


            startService();
            ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;
            timer = new GT.Timer(20000);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            //Carga la ventana principal
            iniciarWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.inicioWindow));
            temperatureWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.temperatureWindow));
            progressBar = (ProgressBar)temperatureWindow.GetChildByName("progress");
            GlideTouch.Initialize();
            text = (TextBlock)iniciarWindow.GetChildByName("text_net_status");
            tmp = (TextBlock)temperatureWindow.GetChildByName("viewTemperature");
            //Inicializa el boton en la interface
            btn_inicio = (Button)iniciarWindow.GetChildByName("button_iniciar");
            btn_inicio.TapEvent += btn_inicio_TapEvent;
            btn_inicio.Enabled = false;

            //Selecciona iniciarWindow como la ventana de inicio
            Glide.MainWindow = iniciarWindow;
            
        }

        void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            btn_inicio.Enabled = true;
            text.Text = ethernetJ11D.NetworkInterface.IPAddress;
            Glide.MainWindow = iniciarWindow;
            Debug.Print("Internet Up");
            timer.Start();
            
        }

        void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            text.Text = "Network OFF";
            Glide.MainWindow = iniciarWindow;
            Debug.Print("No Internet");
            timer.Stop();
        }


        private void timer_Tick(GT.Timer timer)
        {
            HttpRequest request = HttpHelper.CreateHttpGetRequest("http://api.thingspeak.com/channels/46434/fields/2/last");
            request.ResponseReceived += request_ResponseReceived;
            request.SendRequest();
        }

        void request_ResponseReceived(HttpRequest sender, HttpResponse response)
        {

            temperature = response.Text;
            //progressBar.Value =
            tmp.Text = response.Text;

            Glide.MainWindow = temperatureWindow;
            
        }


        void btn_inicio_TapEvent(object sender)
        {
            if (temperature==null)
            {
                tmp.Text = "0";
            }
            else
            {
                tmp.Text = temperature;
            }
            Glide.MainWindow = temperatureWindow;
            Debug.Print("Iniciar");
        }
        void startService()
        {
            ethernetJ11D.NetworkInterface.Open();
            ethernetJ11D.NetworkInterface.EnableDhcp();
            ethernetJ11D.UseThisNetworkInterface();
            Debug.Print(ethernetJ11D.NetworkInterface.IPAddress);
            

        }
    
    }
}
