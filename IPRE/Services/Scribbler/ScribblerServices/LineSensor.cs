//------------------------------------------------------------------------------
// Scribbler Line Sensor Service
//
//     This code was generated by the DssNewService tool.
//
//------------------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using xml = System.Xml;
using soap = W3C.Soap;

using brick = Myro.Services.Scribbler.ScribblerBase.Proxy;
using vector = Myro.Services.Generic.Vector;

namespace Myro.Services.Scribbler.LineSensor
{
    public static class Contract
    {
        public const string Identifier = "http://www.roboteducation.org/schemas/2008/06/scribblerlinesensor.html";
    }

    [DisplayName("Scribbler Line Sensor")]
    [Description("The Scribbler Line Sensor Service")]
    [Contract(Contract.Identifier)]
    [AlternateContract(vector.Contract.Identifier)]
    public class ScribblerLineSensor : vector.VectorServiceBase
    {
        [ServicePort(AllowMultipleInstances = false)]
        vector.VectorOperations _operationsPort = new vector.VectorOperations();
        protected override vector.VectorOperations OperationsPort { get { return _operationsPort; } }

        [Partner("ScribblerBase", Contract = brick.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate, Optional = false)]
        private brick.ScribblerOperations _scribblerPort = new brick.ScribblerOperations();

        private bool _subscribed = false;

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ScribblerLineSensor(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
            _state = new vector.VectorState(
                new List<double> { 0.0, 0.0 },
                new List<string> { "left", "right" },
                DateTime.Now);
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            // display HTTP service Uri
            LogInfo(LogGroups.Console, "Service uri: ");

            SubscribeToScribblerBase();
        }


        /// <summary>
        /// Subscribe to appropriate sensors on Scribbler base
        /// </summary>
        private void SubscribeToScribblerBase()
        {
            // Create a notification port
            brick.ScribblerOperations _notificationPort = new brick.ScribblerOperations();

            //create a custom subscription request
            brick.MySubscribeRequestType request = new brick.MySubscribeRequestType();

            //select only the sensor and port we want
            //NOTE: this name must match the scribbler sensor name.
            request.Sensors = new List<string>();

            request.Sensors.Add("LineLeft");
            request.Sensors.Add("LineRight");

            //Subscribe to the ScribblerBase and wait for a response
            Activate(
                Arbiter.Choice(_scribblerPort.SelectiveSubscribe(request, _notificationPort),
                    delegate(SubscribeResponseType Rsp)
                    {
                        //update our state with subscription status
                        _subscribed = true;

                        LogInfo("Scribbler Line Sensor subscription success");

                        //Subscription was successful, start listening for sensor change notifications
                        Activate(
                            Arbiter.Receive<brick.Replace>(true, _notificationPort, SensorNotificationHandler)
                        );
                    },
                    delegate(soap.Fault F)
                    {
                        LogError("Scribbler Line Sensor subscription failed");
                    }
                )
            );
        }

        /// <summary>
        /// Handle sensor update message from Scribbler
        /// </summary>
        public void SensorNotificationHandler(brick.Replace notify)
        {
            double[] vals = { notify.Body.LineLeft ? 1.0 : 0.0, notify.Body.LineRight ? 1.0 : 0.0 };
            OperationsPort.Post(new vector.SetAllElements(new List<double>(vals)));
        }
    }


}
