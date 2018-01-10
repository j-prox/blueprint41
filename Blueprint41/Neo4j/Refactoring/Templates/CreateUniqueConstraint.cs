﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 15.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Blueprint41.Neo4j.Refactoring.Templates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Blueprint41;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "E:\_Xirqlz\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\CreateUniqueConstraint.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    internal partial class CreateUniqueConstraint : TemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            
            #line 8 "E:\_Xirqlz\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\CreateUniqueConstraint.tt"


    Debug.WriteLine("	executing {0} -> Create unique constraint for {1}.{2}", this.GetType().Name, Entity.Name, Property.Name);

            
            #line default
            #line hidden
            this.Write("CREATE CONSTRAINT ON (node:");
            
            #line 12 "E:\_Xirqlz\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\CreateUniqueConstraint.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Entity.Label.Name));
            
            #line default
            #line hidden
            this.Write(") ASSERT node.");
            
            #line 12 "E:\_Xirqlz\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\CreateUniqueConstraint.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Property.Name));
            
            #line default
            #line hidden
            this.Write(" IS UNIQUE\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 13 "E:\_Xirqlz\blueprint41\Blueprint41\Neo4j\Refactoring\Templates\CreateUniqueConstraint.tt"


	// Template Parameters
	public Entity	Entity	 { get; set; }
	public Property Property { get; set; }


        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
}
