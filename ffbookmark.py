#!/usr/bin/python

import sys, re, json, pprint
from bs4 import BeautifulSoup
from collections import defaultdict

def main(filepath):
	regex_fixdt = r'</A>'
	regex_fixdd = r'(?<!\>)\n<DT>'
	items = tree()
	filltree(items)

	print 'Parsing file',filepath
	
	try:
		# We need the file contents before BeautifulSoup'ing it, because it chokes on this malformed HTML (Too much recursion)
		with open(filepath) as file: 
			filecontent = file.read()

			# Fixing HTML Time !
			# <DT> and <DD> tags are never closed
			filecontent = re.sub(regex_fixdt, fixdt, filecontent)
			filecontent = re.sub(regex_fixdd, fixdd, filecontent)

			soup = BeautifulSoup(filecontent)

			id = 6
			# Get all the links, get their description
			for link in soup.find_all('a'):
				bookmark = {}
				bookmark["title"] = link.contents[0]
				bookmark["id"] = id
				bookmark["parent"] = 5
				bookmark["dateAdded"] = 1
				bookmark["lastModified"] = 1
				bookmark["type"] = "text/x-moz-place"
				bookmark["uri"] = link.get('href')
				id += 1

				try:
					# This line fucks up when the end of the file has been reached
					# Rather than coding properly, let's just catch the exception
					desc = link.parent.next_sibling.next_sibling
					if desc and desc.name == "dd":
						bookmark["annos"] = []
						annos = {}
						annos["name"] = "bookmarkProperties/description"
						annos["flags"] = 0
						annos["expires"] = 4
						annos["mimeType"] = ""
						annos["type"] = 3
						annos["value"] = desc.contents[0]
						bookmark["annos"].append(annos)
				except AttributeError:
					pass
				items["children"][3]["children"].append(bookmark)	

			json_rep = json.dumps(items, indent = 1)
			# Apparently, the max buffer size is 204.800 characters. Let's split that shit
			# Addendum : apparently, 200kb is the max filesize. not good. 
			num_steps = len(json_rep) / 102400
			print "Writing to file in " + str(num_steps) + " steps"
			
			with open('out.json', 'w') as f:
				f.truncate(102400)
				for i in range(0, num_steps):
					print "Pass " + str(i)
					f.write(json_rep[i * 102400:(len(json_rep) - i * 102400) - 1])
					f.flush()

			print "Exported successfully"
	except IOError as e:
		print e
	
def fixdt(matchobj):
	if (matchobj.group(0)) == '</A>': return '</a></dt>'
	else: return ''

def fixdd(matchobj):
	if (matchobj.group(0)): return '</dd>\n<dt>'
	else: return ''

def tree(): return defaultdict(tree)

def filltree(items):	
	#Setting up the base tree!
	items["title"] = ""
	items["id"] = 1
	items["dateAdded"] = 1
	items["lastModified"] = 1
	items["type"] = "text/x-moz-place-container"
	items["root"] = "placesRoot"
	
	items["children"] = []
	items["children"].append(tree())
	items["children"][0]["title"] = "Menu des marques-pages"
	items["children"][0]["id"] = 2
	items["children"][0]["parent"] = 1
	items["children"][0]["dateAdded"] = 1
	items["children"][0]["lastModified"] = 1
	items["children"][0]["type"] = "text/x-moz-place-container"
	items["children"][0]["root"] = "bookmarksMenuFolder"
	items["children"][0]["children"] = []

	items["children"].append(tree())
	items["children"][1]["index"] = 1
	items["children"][1]["title"] = "Barre personnelle"
	items["children"][1]["id"] = 3
	items["children"][1]["parent"] = 1
	items["children"][1]["dateAdded"] = 1
	items["children"][1]["lastModified"] = 1
	items["children"][1]["type"] = "text/x-moz-place-container"
	items["children"][1]["root"] = "toolbarFolder"
	items["children"][1]["children"] = []
	
	items["children"].append(tree())
	items["children"][2]["index"] = 2
	items["children"][2]["title"] = "Etiquettes"
	items["children"][2]["id"] = 4
	items["children"][2]["parent"] = 1
	items["children"][2]["dateAdded"] = 1
	items["children"][2]["lastModified"] = 1
	items["children"][2]["type"] = "text/x-moz-place-container"
	items["children"][2]["root"] = "tagsFolder"
	items["children"][2]["children"] = []

	items["children"].append(tree())
	items["children"][3]["index"] = 3
	items["children"][3]["title"] = "Unsorted Bookmarks"
	items["children"][3]["id"] = 5
	items["children"][3]["parent"] = 1
	items["children"][3]["dateAdded"] = 1
	items["children"][3]["lastModified"] = 1
	items["children"][3]["type"] = "text/x-moz-place-container"
	items["children"][3]["root"] = "unfiledBookmarksFolder"
	items["children"][3]["children"] = []

if __name__ == "__main__":
	if len(sys.argv) > 1:
		main(str(sys.argv[1]))
	else:
		print 'No file to parse'
