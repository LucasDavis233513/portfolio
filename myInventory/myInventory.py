# used to handle the data in a cvs file
import csv

# Imports used to create the
import pyautogui
import customtkinter as ctk
import tkinter as tk
from tkinter import ttk

# Imports the webdriver package from the selenium
from selenium import webdriver

# The following imports are used for the API to scrap Amazon
# Webdrivers for the firefox browser
from selenium.webdriver.firefox.service import Service
from webdriver_manager.firefox import GeckoDriverManager
from selenium.webdriver.common.by import By

# Webdrivers for the chrome browser
from selenium.webdriver.chrome.service import Service
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.common.by import By

# Webdriver options, used to specifythe headless arguemnt (used so the browsers don't open)
from selenium.webdriver.firefox.options import Options as FirefoxOptions
from selenium.webdriver.chrome.options import Options as ChromeOptions

# Used to interpret the exception thrown from the WebDriver
from selenium.common.exceptions import WebDriverException

# Used to add retries with exponential backoff
from retrying import retry

PROGRAMMER_NAME = "Lucas Davis" # Global Programmer name

###################################################################################################
# This will handle reading and writing to the database for this application.
# 
# This class requires the cvs libary to interact with the .cvs file
###################################################################################################
class InvenModel:
    # Construtor
    def __init__(self, file="inventory.csv"):
        self.file = file
        self.data = self.readFromFile()

    # read the database file into a list 
    def readFromFile(self):
        try:
            with open(self.file, 'r') as fl:
                return list(csv.DictReader(fl))
        except IOError:
            return []

    # Write the list back to the file
    def writeToFile(self):
        if(self.data):
            with open(self.file, 'w', newline='') as fl:
                writer = csv.DictWriter(fl, fieldnames=self.data[0].keys())
                writer.writeheader()
                writer.writerows(self.data)

    # Return the number of entries
    def getNumberOfEntries(self):
        return len(self.data)

    # Checks to see if an Entry is present in the database
    def isEntryPresent(self, targetSKU):
        return any(entry['SKU'] == targetSKU for entry in self.data)

    # Add an entry ot the database
    def addDatabaseEntry(self, newEntry):
        if(self.isEntryPresent(newEntry['SKU'])):
            print(f"SKU {newEntry['SKU']} is already present in the database!")
            return
        
        self.data.append(newEntry)
        self.writeToFile()

    # find and return a target entry
    def readDatabase(self, targetSKU):
        for entry in self.data:
            if(entry['SKU'] == targetSKU):
                return entry
        return None

    # Update an entry in the database
    def updateDatabase(self, targetSKU, newEntry):
        if(self.isEntryPresent(targetSKU) == False):
            print(f"SKU {targetSKU} not found in the database!")
            return

        for entry in self.data:
            if(entry['SKU'] == targetSKU):
                entry.update(newEntry)
                self.writeToFile()

    # Remove an entry from the database
    def removeDatabaseEntry(self, targetSKU):
        if(self.isEntryPresent(targetSKU) == False):
            print(f"SKU {targetSKU} is not present in the database")
            return

        self.data = [entry for entry in self.data if entry['SKU'] != targetSKU]
        self.writeToFile()

###################################################################################################
# This will handle the logic to map the user input from the UI to the InvenModel class.
# any button press or input in a text field will call or pass to the model to handle the interaction
# with the database
#
# This class doesn't require any of the libaries
###################################################################################################
class InvenController:
    # Constructor
    def __init__(self, model, view):
        self.model = model
        self.view = view
        self.view.setController(self)

        self.setButtonCmds()

        # Print to the terminal when the app is launching
        print("Checking Browser for the API...")

        # Initalize the scrapper with either the firefox or chrome browsers
        firefoxScraper = WebScraper("firefox")
        if(firefoxScraper.driver):
            self.scraper = firefoxScraper
        else:
            chromeScraper = WebScraper("chrome")
            if(chromeScraper.driver):
                self.scraper = chromeScraper
            else:
                print("Unsupported Browser")
                self.scraper = None

    # This will connfigure the buttons form the view to use the functions from the controller
    def setButtonCmds(self):
        self.view.addEntryButton.configure(command = self.addNewEntry)
        self.view.updateEntryButton.configure(command = self.updateEntry)
        self.view.removeEntryButton.configure(command = self.removeEntry)
        self.view.vendorPriceButton.configure(command = self.vendorPrice)

    def addNewEntry(self):
        entryData = self.view.entryDialog()
        if(entryData):
            self.model.addDatabaseEntry(entryData)
            self.updateView()

    def updateEntry(self):
        entryData = self.view.entryDialog()
        if(entryData):
            self.model.updateDatabase(entryData['SKU'], entryData)
            self.updateView()

    def removeEntry(self):
        entrySKU = self.view.removeEntryDialog()
        if(entrySKU):
            self.model.removeDatabaseEntry(entrySKU)
            self.updateView()

    def vendorPrice(self):
        entries = self.model.readFromFile()
        numOfEntries = self.model.getNumberOfEntries()
        x = 1

        # Calculate the increment value for each 20%
        increment = max(numOfEntries // 5, 1)

        # Create the progress popup window
        self.view.createProgressBar()

        # Calculate the price for each entry in the database
        for entry in entries:
            self.scraper.setItemToSearch(str(entry['Description']))
            price = self.scraper.getWebpage()

            entry['Vendor Price'] = price
            self.model.updateDatabase(entry['SKU'], entry)

            x += 1
            
            # Update progress bar after 20% increments
            if(x % increment == 0):
                progressValue = (x / numOfEntries) * 100
                self.view.progressBar["value"] = progressValue
                self.view.update()

        # Ensure the progress bar reaches 100%
        self.view.progressBar["value"] = 100
        self.view.update()
        
        self.updateView()

        # Destroy the process window after completion
        self.view.progressWindow.destroy()

    # This will call the readDatabase for an entry
    def searchEntry(self, search_text):
        if(not search_text):
            # If the search text is empty, display the entire list
            entries = self.model.readFromFile()
            self.view.updateInventoryDisplay(entries)
        else:
            # Search for the specific entry
            entry = self.model.readDatabase(search_text)
            self.view.updateInventoryDisplay([entry] if entry else [])

    # Updates the inventory field
    def updateView(self):
        # Update the view with the latest data from the model
        entries = self.model.readFromFile()
        self.view.updateInventoryDisplay(entries)   

    # cleanup
    def cleanUp(self):
        if(self.scraper):
            self.scraper.closeDriver()     

###################################################################################################
# This class will create the UI for the myInvenotry application.
# Using three subFrames - sideMenu, mainFrame, and actionFrame - the application will display general
# options in the side menu, action buttons to interact with the UI in the actionFrame, and the inventory
# table in the mainFrame
#
# This class uses the pyautogui, customtkinter, and tkinter libaries
###################################################################################################
class InvenView(ctk.CTk):
    # Constructor
    def __init__(self):
        # Calls the initializer method from the customtkinter class
        super().__init__()

        # Automatically get the screen resolution
        width, height = pyautogui.size()
        resolution = f"{width}x{height}"

        self.title("InvnMVC")               # set the tile of the window
        self.geometry(resolution)          # set the dimenions of the window
        self.controller = None

        # Configure rows and columns
        self.grid_rowconfigure(0, minsize = 100, weight = 1)
        self.grid_rowconfigure(1, minsize = 650, weight = 1)
        self.grid_columnconfigure(0, minsize = 200, weight = 1)
        self.grid_columnconfigure(1, minsize = 1200, weight = 1)

        # Side menu to navigate bewteen different windows
        self.sideMenu = ctk.CTkFrame(self)
        self.sideMenu.grid(row = 0, column = 0, rowspan = 2, padx = 20, pady = 20, sticky = "ns")
        self.createSideMenuUI()

        # This is were we will have the search feild, add new entry, and update price buttons
        self.actionFrame = ctk.CTkFrame(self)
        self.actionFrame.grid(row = 0, column = 1, padx = 100, pady = 10, sticky = "sew") # Strech to fill everything but the top of the subframe
        self.createDefaultActionUI()

        # The main frame where general settings or the invenotry Items will populate
        self.mainFrame = ctk.CTkFrame(self)
        self.mainFrame.grid(row = 1, column = 1, padx = 100, pady = 10, sticky = "nsew") # Strech to fill the entire subframe
        self.createInventoryUI()

    # Create the ActionUI for the actionFrame (This will allow use to interact with the inventory database)
    def createDefaultActionUI(self):
        # Create the searchbox that takes SKU as input to search the database
        self.searchBox = ctk.CTkEntry(self.actionFrame, textvariable = tk.StringVar(), width = 100)
        self.searchBox.insert(0, "Search...")
        self.searchBox.place(relx = 0.1, rely = 0.45, anchor = "s")

        # searchButton to sumbit the SKU to find the entry
        #   Directly call updateInventoryDisplay when the search button is clicked
        self.searchButton = ctk.CTkButton(self.actionFrame, text = "Search")
        self.searchButton.place(relx = 0.1, rely = 0.8, anchor = "s")
        self.searchButton.bind("<Button-1>", self.onSearchButtonClick)

        # Button to open a dialog box to create an entry
        self.addEntryButton = ctk.CTkButton(self.actionFrame, text = "Add New Entry")
        self.addEntryButton.place(relx = 0.3, rely = 0.7, anchor = "s")

        # Button to open a bialog box to update an entry
        self.updateEntryButton = ctk.CTkButton(self.actionFrame, text = "Update Entry")
        self.updateEntryButton.place(relx = 0.5, rely = 0.7, anchor = "s")

        # Button to open a dialog box that will ask for the SKU of the entry to remove
        self.removeEntryButton = ctk.CTkButton(self.actionFrame, text = "Remove Entry")
        self.removeEntryButton.place(relx = 0.7, rely = 0.7, anchor = "s")

        # Button to launch the webScrapper to find the price for the inventory entries
        self.vendorPriceButton = ctk.CTkButton(self.actionFrame, text = "Generate Vendor Price")
        self.vendorPriceButton.place(relx = 0.9, rely = 0.7, anchor = "s")

    # Create the inventoryUI for the mainFrame
    def createInventoryUI(self):
        # Create a header for each of the columns
        headerSKU = ctk.CTkLabel(self.mainFrame, text = "SKU")
        headerSKU.grid(row = 0, column = 0, padx = (0, 10), pady = 10, sticky = "nsew")

        headerDescription = ctk.CTkLabel(self.mainFrame, text = "Description")
        headerDescription.grid(row = 0, column = 1, padx = (0, 10), pady = 10, sticky = "nsew")

        headerQty = ctk.CTkLabel(self.mainFrame, text = "Qty")
        headerQty.grid(row = 0, column = 2, padx = (0, 10), pady = 10, sticky = "nsew")

        headerCost = ctk.CTkLabel(self.mainFrame, text = "Vendor Price")
        headerCost.grid(row = 0, column = 3, padx = (0, 10), pady = 10, sticky = "nsew")

        # This will create a listbox that will allow us to write the database to the UI
        self.entryDisplay = tk.Listbox(self.mainFrame, width = 150, justify = "center")
        self.entryDisplay.grid(row = 1, column = 0, columnspan = 4, padx = 10, pady = 10, sticky = "nsew")

        # The following allows us to expand the entryDisplay to fill the subframe
        # Configure row and column weights to allow expansion
        self.grid_rowconfigure(1, weight = 1)  # Row containing mainFrame
        self.grid_columnconfigure(1, weight = 1)  # Column containing mainFrame

        # Configure row and column weights to allow expansion
        self.mainFrame.grid_rowconfigure(0, weight = 0)  # Header row
        self.mainFrame.grid_rowconfigure(1, weight = 1)  # Entry display row
        self.mainFrame.grid_columnconfigure(0, weight = 1)
        self.mainFrame.grid_columnconfigure(1, weight = 1)
        self.mainFrame.grid_columnconfigure(2, weight = 1)
        self.mainFrame.grid_columnconfigure(3, weight = 1)

    # Create the menu elements for the sidemenu
    #   This will change the mainFrame to focus on the Inventory items or
    #   The vendors used for the API
    def createSideMenuUI(self):
        title = ctk.CTkLabel(self.sideMenu, text = "InvnMVC")
        title.place(relx = 0.5, anchor = "n")

        # Button to change the mainFrame to focus on the Inventory entries
        invnButton = ctk.CTkButton(self.sideMenu, text = "Inventory")
        invnButton.place(relx = 0.5, rely = 0.45, anchor = "center")
        
    # Function to update the inventor UI for the new Entries
    def updateInventoryDisplay(self, entries):
        # variable to define the space between each value
        tab = '                                                                           '

        # Update the listbox with the provided entries
        self.entryDisplay.delete(0, tk.END)

        for entry in entries:
            # Manually concatenate values with spaces
            display_text = tab.join(str(value.get()) 
                                    if isinstance(value, (ctk.CTkLabel, ctk.CTkEntry)) 
                                    else str(value) for value in entry.values())

            self.entryDisplay.insert(tk.END, display_text)

    # Create a progress bar for the scapper operation
    def createProgressBar(self):
        # Create a new popup window for the progress bar
        self.progressWindow = tk.Toplevel(self)
        self.progressWindow.title("Progress bar")

        # Create the prgoress bar within the new window
        self.progressBar = ttk.Progressbar(self.progressWindow, orient = "horizontal", length = 200, mode = "determinate")
        
        self.progressBar.pack(padx = 20, pady = 20)
        self.progressBar["maximum"] = 100
        self.progressBar["value"] = 0
    
    # Dialog box for the add and update functions
    def entryDialog(self):
        keys = ['SKU', 'Description', 'Qty']

        # Create a custom dialog box
        dialog = tk.Toplevel(self)
        dialog.title("Enter Entry Details")

        # Set a dark background for the dialog
        dialog.configure(bg = "black")

        # Create text entry fields for each key in the dictionary
        entries = {}
        for i, key in enumerate(keys):
            label = ctk.CTkLabel(dialog, text=f"{key}:")
            label.grid(row=i, column = 0, padx = 10, pady = 5)

            entry = ctk.CTkEntry(dialog)
            entry.grid(row = i, column = 1, padx = 10, pady = 5)
            entries[key] = entry

        # OK button to confirm the entry
        okButton = ctk.CTkButton(dialog, text = "OK", command = lambda: self.onDialogOK(entries, dialog))
        okButton.grid(row=len(keys), column = 0, columnspan = 2, pady = 10)

        # Create a variable to store the entered data
        entryData = {}

        # Function to handle the OK button click
        def on_ok():
            nonlocal entryData
            entryData = {key: entry.get() for key, entry in entries.items()}
            dialog.destroy()

        okButton.configure(command = on_ok)

        # Wait for the dialog to be closed
        dialog.wait_window()

        # Return the entered data
        return entryData
    
    # Dialog box for the remove function
    def removeEntryDialog(self):
        # Create a custom dialog box for removing an entry
        dialog = tk.Toplevel(self)
        dialog.title("Remove Entry")

        # Set a dark background for the dialog
        dialog.configure(bg = "black")

        # Create a label and entry field for SKU
        label = ctk.CTkLabel(dialog, text="Enter SKU:")
        label.grid(row = 0, column = 0, padx = 10, pady = 5)

        skuEntry = ctk.CTkEntry(dialog)
        skuEntry.grid(row = 0, column = 1, padx = 10, pady = 5)

        # OK button to confirm the SKU and remove the entry
        def on_ok():
            sku = skuEntry.get()
            if(self.controller and sku):
                # Store the SKU value before destroying the dialog
                dialog.skuValue = sku
                dialog.destroy()

        okButton = ctk.CTkButton(dialog, text = "OK", command = on_ok)
        okButton.grid(row=1, column=0, columnspan=2, pady=10)

        # Wait for the dialog to be closed
        dialog.wait_window()

        # Return the SKU entered
        return getattr(dialog, 'skuValue', None)

    def setController(self, controller):
        self.controller = controller

    def onSearchButtonClick(self, event):
        # Notify the controller about the search button click
        if(self.controller):
            self.controller.searchEntry(self.searchBox.get())

###################################################################################################
# WebScrapper is a class useed to scap the first price it finds of an item off of amazons webstore
#   This API WebScrapper is looking for the following HTML segment in Amazons HTML:
#
#       <span class="a-price" data-a-size="xl" data-a-color="base"><span class="a-offscreen">
#       $34.95</span><span aria-hidden="true"><span class="a-price-symbol">$</span>
#       <span class="a-price-whole">34<span class="a-price-decimal">.</span></span>
#       <span class="a-price-fraction">95</span></span></span>
#
# Once found, it will split the $34.95 from the html segement and return it as a string
# This class uses the selenium, webdriver_manager, and retrying libraries
###################################################################################################
class WebScraper():
    # Constructor
    def __init__(self, browerName):
        self.driver = self.check_browser(browerName)

    # Setter for the desired item to search
    def setItemToSearch(self, item):
        self.ITEM_TO_SEARCH = item
        self.URL = f"https://www.amazon.com/s?k={self.ITEM_TO_SEARCH}"

    # Close the drivers
    def closeDriver(self):
        if self.driver:
            self.driver.quit()
            print("WebDriver closed")
    
    # This function will check if the user has either the crhome or firefox browsers installed
    @retry(wait_exponential_multiplier = 1000, wait_exponential_max = 10000, stop_max_attempt_number = 3)
    def check_browser(self, browserName):
        try:
            if(browserName.lower() == "firefox"):
                s = Service(GeckoDriverManager().install())
                options = FirefoxOptions()
            elif(browserName.lower() == "chrome"):
                s = Service(ChromeDriverManager().install())
                options = ChromeOptions()
            else:
                return None # Return None if firefox or chrome wasn't found

            options.add_argument("--headless")  # Run in headless mode (browser doesn't open)
            driver = webdriver.Firefox(service=s, options=options) if browserName.lower() == "firefox" else webdriver.Chrome(service=s, options=options)
            
            return driver
        except WebDriverException as e:
            print(f"Error occurred: {e}") # Return error if there was an error opening the browser
            return None

    # Function to parse the information from the webpage
    @retry(wait_exponential_multiplier = 1000, wait_exponential_max = 10000, stop_max_attempt_number = 3)
    def findPrice(self):
        # Wait for JavaScript to load content
        self.driver.implicitly_wait(10)

        # Find the price
        price_span = self.driver.find_element(By.CSS_SELECTOR, 'span.a-price')
        price = price_span.text.replace('\n', '.')
        return price

    # Function to get the requested webpage
    @retry(wait_exponential_multiplier = 1000, wait_exponential_max = 10000, stop_max_attempt_number = 3)
    def getWebpage(self):
        self.driver.get(self.URL)
        return self.findPrice()

###################################################################################################
# This is the main function that will initialize each class and loop the view
###################################################################################################
if __name__ == "__main__":
    model = InvenModel()
    view = InvenView()
    controller = InvenController(model, view)

    # Update the display with the initial entires
    entries = model.readFromFile()
    view.updateInventoryDisplay(entries)

    # Main loop of the applicaiton
    view.mainloop()

    # Explicitly call the cleanup method before the script ends
    #   Used to clean up the drivers used in the API
    controller.cleanUp()

    # Print the programmers name to the termianl when the app is closed
    print(f"Programmed by: {PROGRAMMER_NAME}")

