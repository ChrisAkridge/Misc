using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.AutoGrouper.Models
{
    internal sealed class PropertyDiagnosticNode
    {
        private readonly List<TypeDiagnosticNode> parents = [];

        public string PropertyName { get; }
        public TypeDiagnosticNode PropertyType { get; }
        public IReadOnlyList<TypeDiagnosticNode> Parents => parents.AsReadOnly();

        public PropertyDiagnosticNode(string propertyName, TypeDiagnosticNode propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        public void AddParent(TypeDiagnosticNode parent)
        {
            ArgumentNullException.ThrowIfNull(parent);
            parents.Add(parent);
        }
    }
}
