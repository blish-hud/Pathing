using System;
using System.ComponentModel;
using System.Drawing.Design;
using BhModule.Community.Pathing.Editor.Panels;
using BhModule.Community.Pathing.Entity;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Editor.TypeEditors {
    public class Vector3Editor : UITypeEditor {

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            if (value is Vector3 vector) {
                MarkerEditWindow.SetPropertyPanel(context.Instance as IPathingEntity, context.PropertyDescriptor.Name, new Vector3PositionToolPanel());
            }

            return base.EditValue(context, provider, value);
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
            return false;
        }

    }
}
