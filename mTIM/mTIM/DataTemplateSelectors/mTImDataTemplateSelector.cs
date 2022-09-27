using System;
using mTIM.Enums;
using mTIM.Models.D;
using Xamarin.Forms;

namespace mTIM.DataTemplateSelectors
{
    public class mTImDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalTypeTemplate { get; set; }
        public DataTemplate AktionTypeTemplate { get; set; }
        public DataTemplate BooleanTypeTemplate { get; set; }
        public DataTemplate DocumentTypeTemplate { get; set; }
        public DataTemplate PrjladenTypeTemplate { get; set; }
        public DataTemplate ValueTypeTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var model = (TimTaskModel)item;
            if (model != null)
            {
                switch (model.Type)
                {
                    case DataType.Int:
                    case DataType.String:
                    case DataType.Float:
                        return ValueTypeTemplate;
                    case DataType.Bool:
                        return BooleanTypeTemplate;
                    case DataType.Doc:
                        return DocumentTypeTemplate;
                    case DataType.Prjladen:
                    case DataType.Prjladen2:
                        return PrjladenTypeTemplate;
                    case DataType.Aktion:
                    case DataType.Aktion2:
                        return AktionTypeTemplate;
                    case DataType.None:
                    case DataType.Referenz:
                    case DataType.Count:
                    default:
                        return NormalTypeTemplate;
                }
            }
            return NormalTypeTemplate;
        }
    }
}
