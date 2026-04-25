using Common;
using System;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for periodic polling.
    /// </summary>
    public class Acquisitor : IDisposable
	{
		private AutoResetEvent acquisitionTrigger;
        private IProcessingManager processingManager;
        private Thread acquisitionWorker;
		private IStateUpdater stateUpdater;
		private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Acquisitor"/> class.
        /// </summary>
        /// <param name="acquisitionTrigger">The acquisition trigger.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="stateUpdater">The state updater.</param>
        /// <param name="configuration">The configuration.</param>
		public Acquisitor(AutoResetEvent acquisitionTrigger, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration)
		{
			this.stateUpdater = stateUpdater;
			this.acquisitionTrigger = acquisitionTrigger;
			this.processingManager = processingManager;
			this.configuration = configuration;
			this.InitializeAcquisitionThread();
			this.StartAcquisitionThread();
		}

		#region Private Methods

        /// <summary>
        /// Initializes the acquisition thread.
        /// </summary>
		private void InitializeAcquisitionThread()
		{
			this.acquisitionWorker = new Thread(Acquisition_DoWork);
			this.acquisitionWorker.Name = "Acquisition thread";
		}

        /// <summary>
        /// Starts the acquisition thread.
        /// </summary>
		private void StartAcquisitionThread()
		{
			acquisitionWorker.Start();
		}

        /// <summary>
        /// Logika akvizicione niti.
        /// Čeka signal od tajmera koji se aktivira svake sekunde, a zatim proverava
        /// svaku konfigurisanu tačku. Kada proteklo vreme za neku tačku dostigne njen
        /// interval očitavanja (3 s za digitalne, 4 s za analogne), u red se dodaje
        /// komanda za čitanje i brojač se resetuje.
        /// </summary>
        private void Acquisition_DoWork()
		{
            //TO DO: IMPLEMENT
            while(true)
            {
                acquisitionTrigger.WaitOne();

                foreach (IConfigItem configItem in configuration.GetConfigurationItems())
                {
                    configItem.SecondsPassedSinceLastPoll++;
                    if (configItem.SecondsPassedSinceLastPoll >= configItem.AcquisitionInterval)
                    {
                        configItem.SecondsPassedSinceLastPoll = 0;

                        processingManager.ExecuteReadCommand(
                            configItem,
                            configuration.GetTransactionId(),
                            configuration.UnitAddress,
                            configItem.StartAddress,
                            configItem.NumberOfRegisters);
                    }
                }
            }
        }

        #endregion Private Methods

        /// <inheritdoc />
        public void Dispose()
		{
			acquisitionWorker.Abort();
        }
	}
}