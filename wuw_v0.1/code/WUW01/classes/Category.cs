using System;//string
using System.Collections;//arraylist
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;//using允许在命名空间中使用类型,
namespace WUW01//name space声明命名空间的名称，并使跟在声明后面的源代码将在该命名空间中进行编译。
{
    public class Category
    {
        private string      _name;//文本即unicode,字符串
        private ArrayList   _prototypes;//AL使用大小会根据需要动态增加的数组？？
/*
public  
访问不受限制。  
protected  
访问仅限于包含类或从包含类派生的类型。  
internal  
访问仅限于当前程序集。  
protected  internal  
访问仅限于从包含类派生的当前程序集或类型。  
private  
访问仅限于包含类型。 
*/
        public Category(string name)
        {
            _name = name;
            _prototypes = null;
        }

        public Category(string name, Gesture firstExample)
        {
            _name = name;
            _prototypes = new ArrayList();//初始化 ArrayList 类的新实例，该实例为空并且具有默认初始容量
            AddExample(firstExample);
        }
        
        public Category(string name, ArrayList examples)
        {
            _name = name;
            _prototypes = new ArrayList(examples.Count);
            for (int i = 0; i < examples.Count; i++)
            {
                Gesture p = (Gesture) examples[i];
                AddExample(p);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }//获取名字

        public int NumExamples
        {
            get
            {
                return _prototypes.Count;
            }
        }//获取原型数

        /// <summary>
        /// Indexer that returns the prototype at the given index within
        /// this gesture category, or null if the gesture does not exist.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Gesture this[int i]
        {
            get
            {
                if (0 <= i && i < _prototypes.Count)
                {
                    return (Gesture) _prototypes[i];
                }
                else
                {
                    return null;
                }
            }
        }//对应种类

        public void AddExample(Gesture p)
        {
            bool success = true;
            try
            {
                // first, ensure that p's name is right首先保证手势的名称正确
                string name = ParseName(p.Name);
                if (name != _name)
                    throw new ArgumentException("Prototype name does not equal the name of the category to which it was added.");

                // second, ensure that it doesn't already exist第二保证它没有已经存在
                for (int i = 0; i < _prototypes.Count; i++)
                {
                    Gesture p0 = (Gesture) _prototypes[i];
                    if (p0.Name == p.Name)
                        throw new ArgumentException("Prototype name was added more than once to its category.");
                }
            }
            catch (ArgumentException ex)//异常处理
            {
                Console.WriteLine(ex.Message);
                success = false;
            }
            if (success)
            {
                _prototypes.Add(p);
            }
        }

        /// <summary>
        /// Pulls the category name from the gesture name, e.g., "circle" from "circle03".
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ParseName(string s)
        {
            string category = String.Empty;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (!Char.IsDigit(s[i]))
                {
                    category = s.Substring(0, i + 1);
                    break;
                }
            }
            return category;
        }

    }
}
