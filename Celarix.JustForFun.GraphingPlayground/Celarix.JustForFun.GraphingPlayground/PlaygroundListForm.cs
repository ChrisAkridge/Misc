using System.Reflection;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal partial class PlaygroundListForm : Form
	{
		private List<Type> playgroundTypes;

		public PlaygroundListForm()
		{
			InitializeComponent();
		}

		private void PlaygroundListForm_Load(object sender, EventArgs e)
		{
			playgroundTypes = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(t => t.Namespace == "Celarix.JustForFun.GraphingPlayground.Playgrounds" && typeof(IPlayground).IsAssignableFrom(t))
				.ToList();

			foreach (var playgroundType in playgroundTypes)
			{
				ListPlaygroundOptions.Items.Add(playgroundType.Name);
			}
		}

		private void ButtonLaunchPlayground_Click(object sender, EventArgs e)
		{
			var selectedTypeName = ListPlaygroundOptions.SelectedItem as string;
			var selectedType = playgroundTypes.Single(t => t.Name == selectedTypeName);

			if (Activator.CreateInstance(selectedType) is not IPlayground instance)
			{
				throw new InvalidOperationException("Playground type was null after construction.");
			}

			var playgroundForm = new PlaygroundForm(instance);
			playgroundForm.Text = instance.Name;
			playgroundForm.ShowDialog();
		}

		private void ListPlaygroundOptions_SelectedIndexChanged(object sender, EventArgs e)
		{
			ButtonLaunchPlayground.Enabled = ListPlaygroundOptions.SelectedItem != null;
		}
	}
}
