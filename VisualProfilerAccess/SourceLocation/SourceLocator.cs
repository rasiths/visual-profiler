using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Cci;

namespace VisualProfilerAccess.SourceLocation
{
    public class SourceLocator : ISourceLocator, IDisposable
    {
        private PdbReader PdbReader { get; set; }
        private Dictionary<uint, IEnumerable<CciMethodLine>> LocationsByToken { get; set; }

        public SourceLocator(string modulePath)
        {
            LocationsByToken = new Dictionary<uint, IEnumerable<CciMethodLine>>();
            PdbReader = new PdbReader(modulePath);
            PopulateSourceLocations();
        }

        private void PopulateSourceLocations()
        {
            IModule module = PdbReader.Module;
            IDisposable disposable = module as IDisposable;
            foreach (var namedTypeDefinition in module.GetAllTypes())
            {
                foreach (var methodDefinition in namedTypeDefinition.Methods)
                {
                    PropertyInfo propertyInfo = methodDefinition.GetType().GetProperty("TokenValue", BindingFlags.NonPublic | BindingFlags.Instance);
                    uint methodMdToken = (uint)propertyInfo.GetValue(methodDefinition, null);

                    List<IPrimarySourceLocation> primarySourceLocations = new List<IPrimarySourceLocation>();
                    foreach (var location in methodDefinition.Locations)
                    {
                        var notNullLocations = PdbReader.GetAllPrimarySourceLocationsFor(location).Where(sl => sl != null);
                        primarySourceLocations.AddRange(notNullLocations);
                    }
                    
                    var cciMethodLines = primarySourceLocations.Select(sl => new CciMethodLine(sl));
                    LocationsByToken[methodMdToken] = cciMethodLines;
                }
            }
        }

        public IEnumerable<IMethodLine> GetMethodLines(uint methodMdToken)
        {
            var methodLines = LocationsByToken[methodMdToken];
            return methodLines;
        }
        
        public string GetSourceFilePath(uint methodMdToken)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
            
            var filePath = LocationsByToken[methodMdToken].First().SourceLocation.PrimarySourceDocument.Location;
            return filePath;
        }
        
        public void Dispose()
        {
            PdbReader.Dispose();
        }
    }
}
