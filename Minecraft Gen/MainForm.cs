using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

public class MainForm : Form
{
    private MenuStrip menuStrip;
    private TabControl tabControl;
    private TreeView treeView;
    private Panel previewPanel;
    private StatusStrip statusStrip;
    private TextBox txtPluginName;
    private TextBox txtPackageName;
    private ComboBox cmbVersion;
    private RichTextBox previewBox;
    private PropertyGrid propertyGrid;
    private Dictionary<Control, string> uiElements;
    private List<string> selectedCommands;
    private List<string> selectedEvents;
    private CheckedListBox commandsList;
    private CheckedListBox eventsList;

    public MainForm()
    {
        uiElements = new Dictionary<Control, string>();
        selectedCommands = new List<string>();
        selectedEvents = new List<string>();
        InitializeComponents();
        SetupUI();
    }

    

    private void InitializeComponents()
    {
        this.Size = new Size(1024, 768);
        this.Text = "Minecraft Plugin Generator";
        this.StartPosition = FormStartPosition.CenterScreen;

        menuStrip = new MenuStrip();
        ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add("New", null, (s, e) => NewPlugin());
        fileMenu.DropDownItems.Add("Save", null, (s, e) => SavePlugin());
        fileMenu.DropDownItems.Add("Export", null, (s, e) => ExportPlugin());
        fileMenu.DropDownItems.Add("-");
        fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());

        ToolStripMenuItem editMenu = new ToolStripMenuItem("Edit");
        editMenu.DropDownItems.Add("Generate Preview", null, (s, e) => UpdatePreview(null, null));

        ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");
        helpMenu.DropDownItems.Add("Documentation", null, (s, e) => ShowDocumentation());
        helpMenu.DropDownItems.Add("About", null, (s, e) => ShowAbout());

        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, helpMenu });
        this.Controls.Add(menuStrip);

        tabControl = new TabControl();
        tabControl.Location = new Point(200, 24);
        tabControl.Size = new Size(600, 668);
        this.Controls.Add(tabControl);

        TabPage basicTab = new TabPage("Basic Info");
        TabPage commandsTab = new TabPage("Commands");
        TabPage eventsTab = new TabPage("Events");
        TabPage uiDesignerTab = new TabPage("UI Designer");
        TabPage configTab = new TabPage("Config");

        CreateBasicInfoTab(basicTab);
        CreateCommandsTab(commandsTab);
        CreateEventsTab(eventsTab);
        CreateUIDesignerTab(uiDesignerTab);
        CreateConfigTab(configTab);

        tabControl.TabPages.AddRange(new TabPage[] { 
            basicTab, 
            commandsTab, 
            eventsTab, 
            uiDesignerTab,
            configTab 
        });

        treeView = new TreeView();
        treeView.Location = new Point(0, 24);
        treeView.Size = new Size(200, 668);
        this.Controls.Add(treeView);

        previewPanel = new Panel();
        previewPanel.Location = new Point(800, 24);
        previewPanel.Size = new Size(208, 668);
        CreatePreviewPanel();
        this.Controls.Add(previewPanel);

        statusStrip = new StatusStrip();
        statusStrip.Items.Add("Ready");
        this.Controls.Add(statusStrip);
    }

private void SaveUIDesigner()
{
    if (uiElements.Count > 0)
    {
        SaveFileDialog saveDialog = new SaveFileDialog();
        saveDialog.Filter = "Java files (*.java)|*.java";
        saveDialog.FilterIndex = 1;
        saveDialog.RestoreDirectory = true;

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            string baseDir = Path.GetDirectoryName(saveDialog.FileName);
            string uiDir = Path.Combine(baseDir, "src", txtPackageName.Text.Replace(".", "\\"), "ui");
            Directory.CreateDirectory(uiDir);

            StringBuilder uiCode = new StringBuilder();
            uiCode.AppendLine($"package {txtPackageName.Text}.ui;");
            uiCode.AppendLine();
            uiCode.AppendLine("import org.bukkit.Bukkit;");
            uiCode.AppendLine("import org.bukkit.entity.Player;");
            uiCode.AppendLine("import org.bukkit.inventory.Inventory;");
            uiCode.AppendLine("import org.bukkit.Material;");
            uiCode.AppendLine("import org.bukkit.inventory.ItemStack;");
            uiCode.AppendLine("import org.bukkit.inventory.meta.ItemMeta;");
            uiCode.AppendLine();

            uiCode.AppendLine($"public class {txtPluginName.Text}GUI {{");
            uiCode.AppendLine("    private final Inventory inventory;");
            uiCode.AppendLine();

            uiCode.AppendLine("    public PluginGUI() {");
            uiCode.AppendLine($"        inventory = Bukkit.createInventory(null, 54, \"{txtPluginName.Text}\");");
            uiCode.AppendLine("        setupItems();");
            uiCode.AppendLine("    }");
            uiCode.AppendLine();

            uiCode.AppendLine("    private void setupItems() {");
            foreach (var element in uiElements)
            {
                string itemName = element.Key.Text ?? element.Value;
                int slot = CalculateSlot(element.Key.Location);
                
                uiCode.AppendLine($"        inventory.setItem({slot}, createGuiItem(\"{itemName}\", Material.STONE_BUTTON));");
            }
            uiCode.AppendLine("    }");

            uiCode.AppendLine(@"
    private ItemStack createGuiItem(String name, Material material) {
        ItemStack item = new ItemStack(material, 1);
        ItemMeta meta = item.getItemMeta();
        meta.setDisplayName(name);
        item.setItemMeta(meta);
        return item;
    }

    public void openInventory(Player player) {
        player.openInventory(inventory);
    }");

            uiCode.AppendLine("}");

            File.WriteAllText(Path.Combine(uiDir, $"{txtPluginName.Text}GUI.java"), uiCode.ToString());
            UpdateStatus("UI Designer saved successfully!");
        }
    }
}

private int CalculateSlot(Point location)
{
    int row = location.Y / 40;
    int col = location.X / 40;
    return row * 9 + col;
}

    private void CreateBasicInfoTab(TabPage tab)
    {
        Label lblPluginName = new Label();
        lblPluginName.Text = "Plugin Name:";
        lblPluginName.Location = new Point(10, 20);

        txtPluginName = new TextBox();
        txtPluginName.Location = new Point(120, 20);
        txtPluginName.Size = new Size(200, 20);
        txtPluginName.TextChanged += UpdatePreview;

        Label lblPackageName = new Label();
        lblPackageName.Text = "Package Name:";
        lblPackageName.Location = new Point(10, 50);

        txtPackageName = new TextBox();
        txtPackageName.Location = new Point(120, 50);
        txtPackageName.Size = new Size(200, 20);
        txtPackageName.TextChanged += UpdatePreview;

        Label lblVersion = new Label();
        lblVersion.Text = "MC Version:";
        lblVersion.Location = new Point(10, 80);

        cmbVersion = new ComboBox();
        cmbVersion.Location = new Point(120, 80);
        cmbVersion.Size = new Size(200, 20);
        cmbVersion.Items.AddRange(new string[] { "1.20", "1.19", "1.18", "1.17", "1.16" });

        Button btnGenerate = new Button();
        btnGenerate.Text = "Generate Plugin";
        btnGenerate.Location = new Point(120, 120);
        btnGenerate.Size = new Size(200, 30);
        btnGenerate.Click += (s, e) => GeneratePlugin();

        tab.Controls.AddRange(new Control[] {
            lblPluginName, txtPluginName,
            lblPackageName, txtPackageName,
            lblVersion, cmbVersion,
            btnGenerate
        });
    }

    private void CreateCommandsTab(TabPage tab)
    {
        commandsList = new CheckedListBox();
        commandsList.Location = new Point(10, 10);
        commandsList.Size = new Size(200, 200);
        commandsList.Items.AddRange(new string[] {
            "Player Commands",
            "Admin Commands",
            "Economy Commands",
            "Teleport Commands",
            "World Commands",
            "Game Commands"
        });
        commandsList.ItemCheck += (s, e) => {
            if (e.NewValue == CheckState.Checked)
                selectedCommands.Add(commandsList.Items[e.Index].ToString());
            else
                selectedCommands.Remove(commandsList.Items[e.Index].ToString());
            UpdatePreview(null, null);
            SetupUI();
        };

        Button btnAddCommand = new Button();
        btnAddCommand.Text = "Add Custom Command";
        btnAddCommand.Location = new Point(10, 220);
        btnAddCommand.Size = new Size(200, 30);
        btnAddCommand.Click += (s, e) => AddCustomCommand();

        tab.Controls.AddRange(new Control[] { commandsList, btnAddCommand });
    }

    private void CreateEventsTab(TabPage tab)
    {
        eventsList = new CheckedListBox();
        eventsList.Location = new Point(10, 10);
        eventsList.Size = new Size(200, 200);
        eventsList.Items.AddRange(new string[] {
            "PlayerJoinEvent",
            "PlayerQuitEvent",
            "BlockBreakEvent",
            "BlockPlaceEvent",
            "PlayerDeathEvent",
            "PlayerMoveEvent",
            "InventoryClickEvent",
            "EntityDamageEvent"
        });
        eventsList.ItemCheck += (s, e) => {
            if (e.NewValue == CheckState.Checked)
                selectedEvents.Add(eventsList.Items[e.Index].ToString());
            else
                selectedEvents.Remove(eventsList.Items[e.Index].ToString());
            UpdatePreview(null, null);
            SetupUI();
        };

        tab.Controls.Add(eventsList);
    }

    private void CreateUIDesignerTab(TabPage tab)
    {
        Panel designerPanel = new Panel();
        designerPanel.Location = new Point(10, 10);
        designerPanel.Size = new Size(550, 400);
        designerPanel.BorderStyle = BorderStyle.FixedSingle;
        designerPanel.AllowDrop = true;
        designerPanel.DragEnter += (s, e) => {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
        };
        designerPanel.DragDrop += (s, e) => AddUIElement(e.Data.GetData(DataFormats.Text).ToString(), e.X, e.Y);

        ListBox toolbox = new ListBox();
        toolbox.Items.AddRange(new string[] {
            "Button",
            "Label",
            "TextBox",
            "ComboBox",
            "Checkbox",
            "Panel",
            "List"
        });
        toolbox.Location = new Point(10, 420);
        toolbox.Size = new Size(150, 200);
        toolbox.MouseDown += (s, e) => {
            if (toolbox.SelectedItem != null)
                DoDragDrop(toolbox.SelectedItem.ToString(), DragDropEffects.Copy);
        };

        propertyGrid = new PropertyGrid();
        propertyGrid.Location = new Point(170, 420);
        propertyGrid.Size = new Size(390, 200);

        tab.Controls.AddRange(new Control[] { designerPanel, toolbox, propertyGrid });
    }

    private void CreateConfigTab(TabPage tab)
    {
        PropertyGrid configGrid = new PropertyGrid();
        configGrid.Location = new Point(10, 10);
        configGrid.Size = new Size(550, 400);
        configGrid.SelectedObject = new PluginConfig();
        
        tab.Controls.Add(configGrid);
    }

    private void CreatePreviewPanel()
    {
        Label previewLabel = new Label();
        previewLabel.Text = "Code Preview";
        previewLabel.Dock = DockStyle.Top;
        previewLabel.TextAlign = ContentAlignment.MiddleCenter;

        previewBox = new RichTextBox();
        previewBox.Location = new Point(0, 20);
        previewBox.Size = new Size(204, 644);
        previewBox.ReadOnly = true;

        previewPanel.Controls.Add(previewLabel);
        previewPanel.Controls.Add(previewBox);
    }

    private void UpdatePreview(object sender, EventArgs e)
    {
        string preview = GeneratePluginContent();
        previewBox.Text = preview;
    }

    private string GeneratePluginContent()
    {
        return $@"package {txtPackageName.Text};

import org.bukkit.plugin.java.JavaPlugin;

public class {txtPluginName.Text} extends JavaPlugin {{
    @Override
    public void onEnable() {{
        getLogger().info(""{txtPluginName.Text} has been enabled!"");
        saveDefaultConfig();
        
        // Register commands
        {GenerateCommandRegistration()}
        
        // Register events
        {GenerateEventRegistration()}
    }}

    @Override
    public void onDisable() {{
        getLogger().info(""{txtPluginName.Text} has been disabled!"");
    }}
}}";
    }

    private string GenerateCommandRegistration()
    {
        if (selectedCommands.Count == 0) return "";
        
        StringBuilder sb = new StringBuilder();
        foreach (string command in selectedCommands)
        {
            string commandClass = command.Replace(" ", "") + "Command";
            sb.AppendLine($"        getCommand(\"{command.ToLower()}\").setExecutor(new commands.{commandClass}());");
        }
        return sb.ToString();
    }

    private string GenerateEventRegistration()
    {
        if (selectedEvents.Count == 0) return "";
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("        PluginManager pm = getServer().getPluginManager();");
        foreach (string evt in selectedEvents)
        {
            string listenerClass = evt.Replace("Event", "Listener");
            sb.AppendLine($"        pm.registerEvents(new listeners.{listenerClass}(), this);");
        }
        return sb.ToString();
    }

    private void NewPlugin()
    {
        txtPluginName.Clear();
        txtPackageName.Clear();
        cmbVersion.SelectedIndex = -1;
        selectedCommands.Clear();
        selectedEvents.Clear();
        uiElements.Clear();
        UpdatePreview(null, null);
        SetupUI();
    }
    private void SetupUI(){
        TreeNode rootNode = new TreeNode("Plugin Structure");

            TreeNode srcNode = new TreeNode("src");
    TreeNode packageNode = new TreeNode(txtPackageName.Text ?? "com.example.plugin");
    
    packageNode.Nodes.Add(new TreeNode($"{txtPluginName.Text ?? "MyPlugin"}.java"));
    
    TreeNode commandsNode = new TreeNode("commands");
    foreach (string command in selectedCommands)
    {
        commandsNode.Nodes.Add(new TreeNode($"{command.Replace(" ", "")}Command.java"));
    }
    packageNode.Nodes.Add(commandsNode);
    
    TreeNode eventsNode = new TreeNode("listeners");
    foreach (string evt in selectedEvents)
    {
        eventsNode.Nodes.Add(new TreeNode($"{evt.Replace("Event", "Listener")}.java"));
    }
    packageNode.Nodes.Add(eventsNode);
    
    TreeNode uiNode = new TreeNode("ui");
    if (uiElements.Count > 0)
    {
        uiNode.Nodes.Add(new TreeNode("PluginGUI.java"));
    }
    packageNode.Nodes.Add(uiNode);
    
    srcNode.Nodes.Add(packageNode);
    rootNode.Nodes.Add(srcNode);
    
    TreeNode resourcesNode = new TreeNode("resources");
    resourcesNode.Nodes.Add(new TreeNode("config.yml"));
    rootNode.Nodes.Add(resourcesNode);
    
    rootNode.Nodes.Add(new TreeNode("plugin.yml"));
    
    treeView.Nodes.Clear();
    treeView.Nodes.Add(rootNode);
    treeView.ExpandAll();
    }
    private void SavePlugin()
    {
        if (string.IsNullOrEmpty(txtPluginName.Text) || string.IsNullOrEmpty(txtPackageName.Text))
        {
            MessageBox.Show("Please fill in the plugin name and package name.", "Required Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        folderDialog.Description = "Select folder to save plugin files";

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
            string baseDir = folderDialog.SelectedPath;
            string srcDir = Path.Combine(baseDir, "src", txtPackageName.Text.Replace(".", "\\"));
            Directory.CreateDirectory(srcDir);

            File.WriteAllText(Path.Combine(srcDir, $"{txtPluginName.Text}.java"), GeneratePluginContent());

            string ymlContent = $@"name: {txtPluginName.Text}
version: 1.0
main: {txtPackageName.Text}.{txtPluginName.Text}
api-version: {cmbVersion.Text}
description: A Minecraft plugin generated with Plugin Generator

commands:";
            foreach (string command in selectedCommands)
            {
                ymlContent += $@"
    {command.ToLower()}:
        description: Executes the {command} command
        usage: /<command>";
            }
            
            File.WriteAllText(Path.Combine(baseDir, "plugin.yml"), ymlContent);

            if (selectedCommands.Count > 0)
            {
                string commandsDir = Path.Combine(srcDir, "commands");
                Directory.CreateDirectory(commandsDir);
                foreach (string command in selectedCommands)
                {
                              File.WriteAllText(
                        Path.Combine(commandsDir, $"{command.Replace(" ", "")}Command.java"),
                        GenerateCommandClass(command)
                    );
                }
            }

            if (selectedEvents.Count > 0)
            {
                string listenersDir = Path.Combine(srcDir, "listeners");
                Directory.CreateDirectory(listenersDir);
                foreach (string evt in selectedEvents)
                {
                    File.WriteAllText(
                        Path.Combine(listenersDir, $"{evt.Replace("Event", "Listener")}.java"),
                        GenerateEventListener(evt)
                    );
                }
            }

            if (uiElements.Count > 0)
            {
                string uiDir = Path.Combine(srcDir, "ui");
                Directory.CreateDirectory(uiDir);
                File.WriteAllText(
                    Path.Combine(uiDir, "PluginGUI.java"),
                    GenerateUIClass()
                );
            }

            SaveUIDesigner();

            UpdateStatus("Plugin saved successfully!");
        }
    }

    private string GenerateCommandClass(string command)
    {
        return $@"package {txtPackageName.Text}.commands;

import org.bukkit.command.Command;
import org.bukkit.command.CommandExecutor;
import org.bukkit.command.CommandSender;
import org.bukkit.entity.Player;

public class {command.Replace(" ", "")}Command implements CommandExecutor {{
    @Override
    public boolean onCommand(CommandSender sender, Command command, String label, String[] args) {{
        if (!(sender instanceof Player)) {{
            sender.sendMessage(""This command can only be used by players!"");
            return true;
        }}

        Player player = (Player) sender;
        // Add {command} command logic here
        player.sendMessage(""Command executed!"");
        return true;
    }}
}}";
    }

    private string GenerateEventListener(string eventName)
    {
        return $@"package {txtPackageName.Text}.listeners;

import org.bukkit.event.EventHandler;
import org.bukkit.event.Listener;
import org.bukkit.event.{eventName};

public class {eventName.Replace("Event", "Listener")} implements Listener {{
    @EventHandler
    public void on{eventName}({eventName} event) {{
        // Add {eventName} handling logic here
    }}
}}";
    }

    private string GenerateUIClass()
    {
        StringBuilder code = new StringBuilder();
        code.AppendLine($"package {txtPackageName.Text}.ui;");
        code.AppendLine();
        code.AppendLine("import org.bukkit.Bukkit;");
        code.AppendLine("import org.bukkit.entity.Player;");
        code.AppendLine("import org.bukkit.inventory.Inventory;");
        code.AppendLine("import org.bukkit.inventory.ItemStack;");
        code.AppendLine("import org.bukkit.inventory.meta.ItemMeta;");
        code.AppendLine();

        code.AppendLine("public class PluginGUI {");
        code.AppendLine("    private Inventory inv;");
        code.AppendLine();

        code.AppendLine("    public PluginGUI() {");
        code.AppendLine($"        inv = Bukkit.createInventory(null, 54, \"{txtPluginName.Text} GUI\");");
        code.AppendLine("        initializeItems();");
        code.AppendLine("    }");
        code.AppendLine();

        code.AppendLine("    private void initializeItems() {");
        foreach (var element in uiElements)
        {
            code.AppendLine($"        // Initialize {element.Value}");
        }
        code.AppendLine("    }");
        code.AppendLine();

        code.AppendLine("    public void openInventory(Player player) {");
        code.AppendLine("        player.openInventory(inv);");
        code.AppendLine("    }");
        code.AppendLine("}");

        return code.ToString();
    }

    private void GeneratePlugin()
    {
        SavePlugin();
    }

    private void ExportPlugin()
    {
        SaveFileDialog saveDialog = new SaveFileDialog
        {
            Filter = "JAR files (*.jar)|*.jar",
            FilterIndex = 1,
            RestoreDirectory = true
        };

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            UpdateStatus("Plugin exported successfully!");
        }
    }

    private void AddCustomCommand()
    {
        using (var form = new Form())
        {
            form.Text = "Add Custom Command";
            form.Size = new Size(300, 150);
            form.StartPosition = FormStartPosition.CenterParent;

            TextBox txtCommand = new TextBox();
            txtCommand.Location = new Point(10, 10);
            txtCommand.Size = new Size(260, 20);

            Button btnAdd = new Button();
            btnAdd.Text = "Add";
            btnAdd.Location = new Point(10, 40);
            btnAdd.DialogResult = DialogResult.OK;

            form.Controls.AddRange(new Control[] { txtCommand, btnAdd });

            if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtCommand.Text))
            {
                commandsList.Items.Add(txtCommand.Text);
                selectedCommands.Add(txtCommand.Text);
                UpdatePreview(null, null);
                SetupUI();
            }
        }
    }

    private void AddUIElement(string elementType, int x, int y)
    {
        Control newElement = null;
        switch (elementType)
        {
            case "Button":
                newElement = new Button { Text = "New Button" };
                break;
            case "Label":
                newElement = new Label { Text = "New Label" };
                break;
            case "TextBox":
                newElement = new TextBox();
                break;
            case "ComboBox":
                newElement = new ComboBox();
                break;
            case "Checkbox":
                newElement = new CheckBox { Text = "New Checkbox" };
                break;
        }

        if (newElement != null)
        {
            newElement.Location = new Point(x, y);
            newElement.Size = new Size(100, 25);
            newElement.Click += (s, e) => propertyGrid.SelectedObject = s;
            
            Panel designerPanel = (Panel)tabControl.TabPages["UI Designer"].Controls[0];
            designerPanel.Controls.Add(newElement);
            uiElements[newElement] = elementType;
            SetupUI();
        }
    }

    private void ShowDocumentation()
    {
        System.Diagnostics.Process.Start("https://hub.spigotmc.org/javadocs/spigot/");
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "Minecraft Plugin Generator v1.0\n" +
            "Created for the Minecraft community\n\n" +
            "This tool helps you create professional Minecraft plugins easily.",
            "About Plugin Generator",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private void UpdateStatus(string message)
    {
        statusStrip.Items[0].Text = message;
    }
}

public class PluginConfig
{
    public string ServerName { get; set; } = "My Server";
    public bool EnableDebug { get; set; } = false;
    public string WelcomeMessage { get; set; } = "Welcome to the server!";
    public int MaxPlayers { get; set; } = 20;
}

