def find_document(collection, elements, multiple=False, retfields = None):
    if multiple:
        results = collection.find(elements, retfields)
        return [r for r in results]
    else:
        return collection.find_one(elements)
    
def update_document(collection, query_elements, new_values):
    """ Function to update a single document in a collection.
    """
    collection.update_one(query_elements, {'$set': new_values})

def insert_document(collection, data):
    """ Function to insert a document into a collection and
    return the document's id.
    """

    return collection.insert_one(data).inserted_id

def delete_document(collection, query):
    """ Function to delete a single document from a collection.
    """
    collection.delete_one(query)