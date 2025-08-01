import { Component, TemplateRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormControl } from '@angular/forms';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import {
  TodoListsClient, TodoItemsClient,
  TodoListDto, TodoItemDto, PriorityLevelDto,
  CreateTodoListCommand, UpdateTodoListCommand,
  CreateTodoItemCommand, UpdateTodoItemDetailCommand
} from '../web-api-client';

@Component({
  selector: 'app-todo-component',
  templateUrl: './todo.component.html',
  styleUrls: ['./todo.component.scss']
})
export class TodoComponent implements OnInit {
  debug = false;
  deleting = false;
  deleteCountDown = 0;
  deleteCountDownInterval: any;
  lists: TodoListDto[];
  priorityLevels: PriorityLevelDto[];
  selectedList: TodoListDto;
  selectedItem: TodoItemDto;
  newListEditor: any = {};
  listOptionsEditor: any = {};
  newListModalRef: BsModalRef;
  listOptionsModalRef: BsModalRef;
  deleteListModalRef: BsModalRef;
  itemDetailsModalRef: BsModalRef;

  tagInputControl = new FormControl('');
  tags: string[] = [];
  tagInput = '';
  selectedTagFilters: string[] = [];
  searchQuery: string = '';

  itemDetailsFormGroup = this.fb.group({
    id: [null],
    listId: [null],
    priority: [''],
    note: [''],
    colour: [''],
    tags: this.fb.control([]),
  });

  allowedColors = [
    { name: 'White', value: '#FFFFFF' },
    { name: 'Light Gray', value: '#F5F5F5' },
    { name: 'Gray', value: '#E0E0E0' },
    { name: 'Light Blue', value: '#E3F2FD' },
    { name: 'Sky Blue', value: '#BBDEFB' },
    { name: 'Cornflower Blue', value: '#90CAF9' },
    { name: 'Azure', value: '#64B5F6' },
    { name: 'Dodger Blue', value: '#42A5F5' },
    { name: 'Light Green', value: '#E8F5E9' },
    { name: 'Mint', value: '#C8E6C9' },
    { name: 'Pastel Green', value: '#A5D6A7' },
    { name: 'Soft Green', value: '#81C784' },
    { name: 'Light Yellow', value: '#FFFDE7' },
    { name: 'Pale Yellow', value: '#FFF9C4' },
    { name: 'Banana Yellow', value: '#FFF59D' },
    { name: 'Bright Yellow', value: '#FFEB3B' },
    { name: 'Rose', value: '#FFEBEE' },
    { name: 'Blush Pink', value: '#FFCDD2' },
    { name: 'Light Coral', value: '#EF9A9A' },
    { name: 'Soft Red', value: '#E57373' },
    { name: 'Lavender', value: '#F3E5F5' },
    { name: 'Light Purple', value: '#E1BEE7' },
    { name: 'Mauve', value: '#CE93D8' },
    { name: 'Orchid', value: '#BA68C8' },
  ];

  constructor(
    private listsClient: TodoListsClient,
    private itemsClient: TodoItemsClient,
    private modalService: BsModalService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.listsClient.get().subscribe(
      result => {
        this.lists = result.lists;
        console.log('Todo lists:', this.lists);
        this.priorityLevels = result.priorityLevels;
        if (this.lists.length) {
          this.selectedList = this.lists[0];
        }
      },
      error => console.error(error)
    );
  }

  // Lists
  remainingItems(list: TodoListDto): number {
    return list.items.filter(t => !t.done).length;
  }

  showNewListModal(template: TemplateRef<any>): void {
    this.newListModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('title').focus(), 250);
  }

  newListCancelled(): void {
    this.newListModalRef.hide();
    this.newListEditor = {};
  }

  addList(): void {
    const list = {
      id: 0,
      title: this.newListEditor.title,
      items: []
    } as TodoListDto;

    this.listsClient.create(list as CreateTodoListCommand).subscribe(
      result => {
        list.id = result;
        this.lists.push(list);
        this.selectedList = list;
        this.newListModalRef.hide();
        this.newListEditor = {};
      },
      error => {
        const errors = JSON.parse(error.response);

        if (errors && errors.Title) {
          this.newListEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('title').focus(), 250);
      }
    );
  }

  showListOptionsModal(template: TemplateRef<any>) {
    this.listOptionsEditor = {
      id: this.selectedList.id,
      title: this.selectedList.title
    };

    this.listOptionsModalRef = this.modalService.show(template);
  }

  updateListOptions() {
    const list = this.listOptionsEditor as UpdateTodoListCommand;
    this.listsClient.update(this.selectedList.id, list).subscribe(
      () => {
        (this.selectedList.title = this.listOptionsEditor.title),
          this.listOptionsModalRef.hide();
        this.listOptionsEditor = {};
      },
      error => console.error(error)
    );
  }

  confirmDeleteList(template: TemplateRef<any>) {
    this.listOptionsModalRef.hide();
    this.deleteListModalRef = this.modalService.show(template);
  }

  deleteListConfirmed(): void {
    this.listsClient.delete(this.selectedList.id).subscribe(
      () => {
        this.deleteListModalRef.hide();
        this.lists = this.lists.filter(t => t.id !== this.selectedList.id);
        this.selectedList = this.lists.length ? this.lists[0] : null;
      },
      error => console.error(error)
    );
  }

  // Items
  showItemDetailsModal(template: TemplateRef<any>, item: TodoItemDto): void {
    this.selectedItem = item;
    console.log('Selected item:', this.selectedItem);
    // Set form values
    this.itemDetailsFormGroup.patchValue(this.selectedItem);

    // Initialize local tags array (used for rendering)
    this.tags = item.tags ? [...item.tags] : [];
    this.itemDetailsFormGroup.get('tags')?.setValue(this.tags);

    this.itemDetailsModalRef = this.modalService.show(template);
    this.itemDetailsModalRef.onHidden.subscribe(() => {
      this.stopDeleteCountDown();
    });
  }


  updateItemDetails(): void {
    this.itemDetailsFormGroup.get('tags')?.setValue(this.tags);
    const item = new UpdateTodoItemDetailCommand(this.itemDetailsFormGroup.value);
    console.log(item);
    this.itemsClient.updateItemDetails(this.selectedItem.id, item).subscribe(
      () => {
        if (this.selectedItem.listId !== item.listId) {
          this.selectedList.items = this.selectedList.items.filter(
            i => i.id !== this.selectedItem.id
          );
          const listIndex = this.lists.findIndex(
            l => l.id === item.listId
          );
          this.selectedItem.listId = item.listId;
          this.lists[listIndex].items.push(this.selectedItem);
        }

        this.selectedItem.priority = item.priority;
        this.selectedItem.note = item.note;
        this.selectedItem.colour = item.colour;
        this.selectedItem.tags = item.tags;

        this.itemDetailsModalRef.hide();
        this.itemDetailsFormGroup.reset();
      },
      error => console.error(error)
    );
  }

  addItem() {
    const item = {
      id: 0,
      listId: this.selectedList.id,
      priority: this.priorityLevels[0].value,
      title: '',
      done: false
    } as TodoItemDto;

    this.selectedList.items.push(item);
    const index = this.selectedList.items.length - 1;
    this.editItem(item, 'itemTitle' + index);
  }

  editItem(item: TodoItemDto, inputId: string): void {
    this.selectedItem = item;
    setTimeout(() => document.getElementById(inputId).focus(), 100);
  }

  updateItem(item: TodoItemDto, pressedEnter: boolean = false): void {
    const isNewItem = item.id === 0;

    if (!item.title.trim()) {
      this.deleteItem(item);
      return;
    }

    if (item.id === 0) {
      this.itemsClient
        .create({
          ...item, listId: this.selectedList.id
        } as CreateTodoItemCommand)
        .subscribe(
          result => {
            item.id = result;
          },
          error => console.error(error)
        );
    } else {
      this.itemsClient.update(item.id, item).subscribe(
        () => console.log('Update succeeded.'),
        error => console.error(error)
      );
    }

    this.selectedItem = null;

    if (isNewItem && pressedEnter) {
      setTimeout(() => this.addItem(), 250);
    }
  }

  deleteItem(item: TodoItemDto, countDown?: boolean) {
    if (countDown) {
      if (this.deleting) {
        this.stopDeleteCountDown();
        return;
      }
      this.deleteCountDown = 3;
      this.deleting = true;
      this.deleteCountDownInterval = setInterval(() => {
        if (this.deleting && --this.deleteCountDown <= 0) {
          this.deleteItem(item, false);
        }
      }, 1000);
      return;
    }
    this.deleting = false;
    if (this.itemDetailsModalRef) {
      this.itemDetailsModalRef.hide();
    }

    if (item.id === 0) {
      const itemIndex = this.selectedList.items.indexOf(this.selectedItem);
      this.selectedList.items.splice(itemIndex, 1);
    } else {
      this.itemsClient.delete(item.id).subscribe(
        () =>
        (this.selectedList.items = this.selectedList.items.filter(
          t => t.id !== item.id
        )),
        error => console.error(error)
      );
    }
  }

  stopDeleteCountDown() {
    clearInterval(this.deleteCountDownInterval);
    this.deleteCountDown = 0;
    this.deleting = false;
  }

  addTag(): void {
    const newTag = this.tagInputControl.value?.trim();

    if (newTag && !this.tags.includes(newTag)) {
      this.tags.push(newTag);
      this.itemDetailsFormGroup.get('tags')?.setValue(this.tags);
    }

    this.tagInputControl.reset();
  }

  removeTag(tagToRemove: string): void {
    this.tags = this.tags.filter(tag => tag !== tagToRemove);
    this.itemDetailsFormGroup.get('tags')?.setValue(this.tags);
  }

  toggleTagFilter(tag: string): void {
    const index = this.selectedTagFilters.indexOf(tag);
    if (index === -1) {
      this.selectedTagFilters.push(tag);
    } else {
      this.selectedTagFilters.splice(index, 1);
    }
  }

  clearTagFilters(): void {
    this.selectedTagFilters = [];
  }

  get filteredItems(): TodoItemDto[] {
    const items = this.selectedList?.items ?? [];

    return items.filter(item => {
      const matchesTags =
        this.selectedTagFilters.length === 0 ||
        item.tags?.some(tag => this.selectedTagFilters.includes(tag));

      const query = this.searchQuery.trim().toLowerCase();
      const matchesSearch =
        !query ||
        (item.title?.toLowerCase().includes(query) ||
          item.note?.toLowerCase().includes(query) ||
          item.tags?.some(tag => tag.toLowerCase().includes(query)));

      return matchesTags && matchesSearch;
    });
  }

  getAllTags(): string[] {
    const tagSet = new Set<string>();

    for (const item of this.selectedList?.items ?? []) {
      if (item.tags) {
        for (const tag of item.tags) {
          tagSet.add(tag);
        }
      }
    }

    return Array.from(tagSet).sort();
  }


}
