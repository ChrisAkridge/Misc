using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SystemAll
{
    public sealed class SystemAll
    {
        public void TheMethod()
        {
            // Challenge: Use every publicly accessible class and structure in the System namespace in a useful way, all in one method.

            // We'll save exceptions for the end

            // System.ActivationContext
            Console.WriteLine("System.ActivationContext:");
            // This one is really not easy to make, so we'll just reflect the constructor
            ActivationContext context = (ActivationContext)typeof(ActivationContext).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(null);
            ApplicationIdentity identity = (ApplicationIdentity)typeof(ApplicationIdentity).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(null);
            var contextIdentityField = typeof(ActivationContext).GetField("_applicationIdentity", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            contextIdentityField.SetValue(context, identity);
            Console.WriteLine("Form: {0}", context.Form);
            WriteInputRequest();

            // System.Activator
            Console.WriteLine("System.Activator");
            string s = (string)Activator.CreateInstance(typeof(string), new char[] { 'H', 'e', 'l', 'l', 'o'});
            Random random = (Random)Activator.CreateInstance<Random>();
            int seed = ((int[])typeof(Random).GetField("SeedArray", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(random))[random.Next(0, 56)];
            Console.WriteLine("Activator-created string: {0}", s);
            Console.WriteLine("Value from Activator-created RNG: {0} (random seed: {1})", random.Next(), seed);
            WriteInputRequest();

            // System.AppDomain
            Console.WriteLine("System.AppDomain");
            AppDomain domain = AppDomain.CurrentDomain;
            Console.WriteLine("ID: {0}", domain.Id);
            Console.WriteLine("Base directory: {0}", domain.BaseDirectory);
            Console.WriteLine("Is fully trusted? {0}", (domain.IsFullyTrusted) ? "Yes" : "No :(");
            WriteInputRequest();

            // System.AppDomainManager
            Console.WriteLine("System.AppDomainManager");
            AppDomainManager manager = new AppDomainManager();
			var objectRef = manager.CreateObjRef(typeof(int));
			Console.WriteLine($"Channel: {objectRef.ChannelInfo.ToString()}\r\nEnvoy: {objectRef.EnvoyInfo.ToString()}");
        }

        private void WriteBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Console.WriteLine("Null or empty array.");
                return;
            }

            foreach (byte b in bytes)
            {
                Console.Write("{0} ", b);
            }
        }

        private void WriteInputRequest()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
