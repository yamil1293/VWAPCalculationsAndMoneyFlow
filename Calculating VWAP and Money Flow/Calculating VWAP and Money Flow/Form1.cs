// Form1.cs
// Example Bloomberg API C# Subscriptions Program in Windows Forms
// (C) 2014 Richard Holowczak
// Portions (c) Bloomberg, LLC

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// Bloomberg API Namespaces
using Element = Bloomberglp.Blpapi.Element;
using Event = Bloomberglp.Blpapi.Event;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using EventQueue = Bloomberglp.Blpapi.EventQueue;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using Subscription = Bloomberglp.Blpapi.Subscription;
using Request = Bloomberglp.Blpapi.Request; 

namespace CIS_4620_Financial_IT_Assignment_3
{
    public partial class Form1 : Form
    {
        // Define a SessionOptions, Session and subscriptions object
        // These are used throughout the form
        Session session;
        SessionOptions sessionOptions;
        List<Subscription> subscriptions;
        
        public Form1()
        {
            InitializeComponent();
        }

        // Event handler for the session
        public void processBBEvent(Event eventObject, Session session)
        {
            // We are only interested in market data events related to
            // our subscription
            if (eventObject.Type == Event.EventType.SUBSCRIPTION_DATA)
            {
                // Each event may have multiple messages. So loop
                // through all of the messages
                foreach (Message msg in eventObject)
                {
                    // For Debugging use the display all of the fields: 
                    System.Console.WriteLine(msg); 
                    // If we have both LAST_TRADE and SIZE_LAST_TRADE elements then
                    // assign the values to the text boxes
                    if (msg.HasElement("LAST_TRADE") &&
                        msg.HasElement("SIZE_LAST_TRADE"))
                    {
                        // Obtain the topic of the message
                        string topic = (string)msg.CorrelationID.Object;
                        try
                        {
                            tbPrice.Invoke(new MethodInvoker(delegate
                            {
                                tbPrice.Text = (string)msg.GetElementAsString("LAST_TRADE");
                            }));
                            tbVolume.Invoke(new MethodInvoker(delegate
                            {
                                tbVolume.Text = (string)msg.GetElementAsString("SIZE_LAST_TRADE");
                            }));
                        }
                        catch (Exception e)
                        {
                            // Send an error message to console 
                            System.Console.WriteLine("Error: " + e.ToString());
                        }
                    } // end if msg has both last trade and size
                } // end loop over Messages in the eventObject
            } // end if event type is subscription data
        } // end processBBEvent

        private void btnStart_Click(object sender, EventArgs e)
        {
            bool result;
            // Define two string constants for the security and the fields
            // to be read. 
            String security = "IBM US Equity";
            String fields = "LAST_PRICE";
            // Instantiate the SessionOptions object to hold the session parameters
            // Note that this was defined at the Form level scope.
            sessionOptions = new SessionOptions();
            // Since this program will run on the same PC as the Bloomberg software,
            //  we use “localhost” as the host name and port 8194 as the default port.
            sessionOptions.ServerHost = "localhost";
            sessionOptions.ServerPort = 8194;
            // Instantiate the Session object using the sessionOptions and
            // a reference to the event handler named processBBEvent
            session = new Session(sessionOptions,
                      new Bloomberglp.Blpapi.EventHandler(processBBEvent));
            // Start the Session
            result = session.Start();
            // Open up the Market data Service
            result = session.OpenService("//blp/mktdata");
            // Instantiate the new list of subscriptions
            subscriptions = new List<Subscription>();
            // Get the symbol from tbSymbol text box
            security = tbSymbol.Text;
            // Add a subscription. Create a new CorrelationID on the fly
            subscriptions.Add(new Subscription(security, fields, "",
                              new Bloomberglp.Blpapi.CorrelationID(security)));
            // Kick off the subscriptions
            session.Subscribe(subscriptions);
  
        } // end btnStart_Click
            
            //Declaring variables for calculations
            double dblSymbol;
            double dblPrice;
            double dblVolume;
            double dblVWAP;

       private void btnStop_Click(object sender, EventArgs e)
        {
            // Turn off the subscription
            session.Unsubscribe(subscriptions);
            // Close the session
            session.Stop();
            // Clear text boxes
            tbPrice.Text = String.Empty;
            tbVolume.Text = String.Empty;
        } // end btStart_Click
    } // end public partial class Form1 
} // end namespace Bloomberg_API_csharp_WinForms_RealTime
