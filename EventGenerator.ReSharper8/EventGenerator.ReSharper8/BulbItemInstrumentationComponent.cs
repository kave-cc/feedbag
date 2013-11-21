﻿using System.Linq;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Intentions.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using KaVE.EventGenerator.ReSharper8.MessageBus;
using KaVE.EventGenerator.ReSharper8.VsIntegration;
using KaVE.Utils.Assertion;

namespace KaVE.EventGenerator.ReSharper8
{
    [SolutionComponent]
    internal class BulbItemInstrumentationComponent : IBulbItemsProvider
    {
        private readonly IVsDTE _dte;
        private readonly IMessageBus _messageBus;

        public BulbItemInstrumentationComponent(IVsDTE dte, IMessageBus messageBus)
        {
            _dte = dte;
            _messageBus = messageBus;
        }

        public int Priority
        {
            get
            {
                // to make sure we catch all bulb actions, we place this provider at the last possible
                // possition in the queue
                return int.MaxValue;
            }
        }

        public object PreExecute(ITextControl textControl)
        {
            return null;
        }

        public void CollectActions(IntentionsBulbItems intentionsBulbItems,
            BulbItems.BulbCache cacheData,
            ITextControl textControl,
            Lifetime caretPositionLifetime,
            IPsiSourceFile psiSourceFile,
            object precalculatedData)
        {
            var allBulbMenuItems = intentionsBulbItems.AllBulbMenuItems;
            foreach (var executableItem in allBulbMenuItems.Select(item => item.ExecutableItem))
            {
                var proxy = executableItem as IntentionAction.MyExecutableProxi;
                if (proxy != null)
                {
                    proxy.WrapBulbAction(_dte, _messageBus);
                    continue;
                }

                var executable = executableItem as ExecutableItem;
                if (executable != null)
                {
                    executable.WrapBulbAction(_dte, _messageBus);
                    continue;
                }

                Asserts.Fail("unexpected item type: {0}", executableItem.GetType().FullName);
            }
        }
    }
}